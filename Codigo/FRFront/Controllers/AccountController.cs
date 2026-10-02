using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using FRFront.Helpers;
using System.Net.Http.Json;

namespace FRFront.Controllers
{
    public class AccountController : Controller
    {
        private sealed class LoginApiResponse
        {
            public string Token { get; set; } = string.Empty;
            public int IdUsuario { get; set; }
            public string TipoUsuario { get; set; } = string.Empty;
        }

        private readonly HttpClient _httpClient;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            LoginApiResponse? login;
            try
            {
                using var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
                {
                    Email = model.Email.Trim(),
                    Password = model.Password
                });

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ViewData["ReturnUrl"] = returnUrl;
                    ModelState.AddModelError("Password", "La contraseña ingresada es incorrecta.");
                    return View(model);
                }

                if (!response.IsSuccessStatusCode)
                {
                    ViewData["ReturnUrl"] = returnUrl;
                    ModelState.AddModelError(string.Empty, "No se pudo validar la cuenta.");
                    return View(model);
                }

                login = await response.Content.ReadFromJsonAsync<LoginApiResponse>();
            }
            catch (HttpRequestException)
            {
                ViewData["ReturnUrl"] = returnUrl;
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servidor de autenticación.");
                return View(model);
            }

            if (login == null || login.IdUsuario <= 0)
            {
                ViewData["ReturnUrl"] = returnUrl;
                ModelState.AddModelError(string.Empty, "La respuesta de autenticación no es válida.");
                return View(model);
            }

            HttpContext.Session.SetString("UsuarioSesion", model.Email.Trim());
            HttpContext.Session.SetString("RolSesion", login.TipoUsuario);
            HttpContext.Session.SetString("UsuarioApiId", login.IdUsuario.ToString());

            if (string.Equals(login.TipoUsuario, "Cliente", StringComparison.OrdinalIgnoreCase))
            {
                HttpContext.Session.SetString("RolSesion", "Cliente");
                HttpContext.Session.SetString("EstadoCliente", "ACTIVO");
                HttpContext.Session.SetString("UltimoIngreso", DateTime.Now.ToString("o"));
                await RestaurarCarritoApiAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            var usuario = HttpContext.Session.GetString("UsuarioSesion")?.Trim().ToLowerInvariant();
            var carrito = !string.IsNullOrWhiteSpace(usuario)
                ? HttpContext.Session.GetString($"CarritoSession:{usuario}")
                : null;
            var historial = !string.IsNullOrWhiteSpace(usuario)
                ? HttpContext.Session.GetString($"HistorialComprasSession:{usuario}")
                : null;
            var pedidoPersistido = !string.IsNullOrWhiteSpace(usuario)
                ? HttpContext.Session.GetString($"PedidoPersistidoSesion:{usuario}")
                : null;
            var usuarioApiId = HttpContext.Session.GetString("UsuarioApiId");

            HttpContext.Session.Clear();

            if (!string.IsNullOrWhiteSpace(usuario))
            {
                if (carrito != null)
                    HttpContext.Session.SetString($"CarritoSession:{usuario}", carrito);
                if (historial != null)
                    HttpContext.Session.SetString($"HistorialComprasSession:{usuario}", historial);
                if (pedidoPersistido != null)
                    HttpContext.Session.SetString($"PedidoPersistidoSesion:{usuario}", pedidoPersistido);
            }
            if (usuarioApiId != null)
                HttpContext.Session.SetString("UsuarioApiId", usuarioApiId);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", new
                {
                    Nombre = model.Nombre,
                    Apellido = model.Nombre,
                    Email = model.Email.Trim(),
                    Password = model.Password
                });

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError("Email", "El correo ya está registrado.");
                    return View(model);
                }

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "No se pudo guardar la cuenta en la base de datos.");
                    return View(model);
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con la base de datos.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Registro completado con éxito. Ya puedes iniciar sesión.";
            return RedirectToAction("Login", "Account");
        }

        private async Task RestaurarCarritoApiAsync()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UsuarioApiId")))
                return;

            var claveCarrito = UserSessionKeys.ForUser(HttpContext.Session, "CarritoSession");
            if (!int.TryParse(HttpContext.Session.GetString("UsuarioApiId"), out var usuarioId))
                return;

            try
            {
                var carrito = await _httpClient.GetFromJsonAsync<List<ItemCarrito>>($"api/usuarios/{usuarioId}/carrito") ?? new List<ItemCarrito>();
                HttpContext.Session.SetString(claveCarrito, JsonSerializer.Serialize(carrito));
            }
            catch (HttpRequestException)
            {
                // Se conserva el valor local solamente si la API no está disponible.
            }
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ViewBag.MensajeExito = "Si el correo ingresado coincide con una cuenta registrada, recibirás un enlace de restablecimiento.";
            return View();
        }

        // GET: /Account/CambiarContrasena
        [HttpGet]
        public IActionResult CambiarContrasena()
        {
            return View("~/Views/Account/CambiarContrasena.cshtml", new CambiarContrasenaViewModel());
        }

        // POST: /Account/CambiarContrasena
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarContrasena(CambiarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/CambiarContrasena.cshtml", model);
            }

            TempData["SuccessMessage"] = $"Se envió un correo de recuperación a {model.Email}.";
            return RedirectToAction("CambiarContrasena");
        }
    }
}