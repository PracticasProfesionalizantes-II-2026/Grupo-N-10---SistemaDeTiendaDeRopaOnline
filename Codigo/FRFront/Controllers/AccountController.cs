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

            string emailLower = model.Email.ToLower();

            // 1. Obtener la contraseña guardada previamente en la sesión o la predeterminada "Admin123"
            string claveEsperada = HttpContext.Session.GetString("PasswordSesion") ?? "Admin123";

            // 2. Validar que la contraseña coincida con la activa
            if (model.Password != claveEsperada)
            {
                ViewData["ReturnUrl"] = returnUrl;
                ModelState.AddModelError("Password", "La contraseña ingresada es incorrecta.");
                return View(model);
            }

            // 3. Persistir usuario y clave en sesión
            HttpContext.Session.SetString("UsuarioSesion", model.Email);
            HttpContext.Session.SetString("PasswordSesion", claveEsperada);

            // 4. Redirección por Roles
            if (emailLower.Contains("admin"))
            {
                HttpContext.Session.SetString("RolSesion", "Administrador");
                await AsegurarUsuarioApiAsync(model.Email, model.Password, 3);
                await RestaurarCarritoApiAsync();
                return RedirectToAction("Index", "Home");
            }
            else if (emailLower.Contains("empleado") || emailLower.Contains("cajero"))
            {
                HttpContext.Session.SetString("RolSesion", "Empleado");
                await AsegurarUsuarioApiAsync(model.Email, model.Password, 4);
                await RestaurarCarritoApiAsync();
                return RedirectToAction("Index", "Home");
            }
            else
            {
                bool estaBloqueado = emailLower.Contains("bloqueado");

                if (estaBloqueado)
                {
                    ViewData["ReturnUrl"] = returnUrl;
                    ModelState.AddModelError(string.Empty, "Su cuenta se encuentra BLOQUEADA. Por favor, contacte con soporte.");
                    return View(model);
                }

                HttpContext.Session.SetString("RolSesion", "Cliente");
                HttpContext.Session.SetString("EstadoCliente", "ACTIVO");
                HttpContext.Session.SetString("UltimoIngreso", DateTime.Now.ToString("o"));
                await AsegurarUsuarioApiAsync(model.Email, model.Password, 5);
                await RestaurarCarritoApiAsync();

                // Si hay una ruta de retorno válida (ej. el carrito), volvemos ahí
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            var usuario = HttpContext.Session.GetString("UsuarioSesion")?.Trim().ToLowerInvariant();
            var claveGuardada = HttpContext.Session.GetString("PasswordSesion");
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

            if (!string.IsNullOrEmpty(claveGuardada))
            {
                HttpContext.Session.SetString("PasswordSesion", claveGuardada);
            }

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
                var response = await _httpClient.PostAsJsonAsync("api/usuarios", new
                {
                    Nombre = model.Nombre,
                    Apellido = model.Nombre,
                    Email = model.Email,
                    Password = model.Password,
                    Rol = 5,
                    Activo = true
                });

                if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Conflict)
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

        private async Task AsegurarUsuarioApiAsync(string email, string password, int rol)
        {
            try
            {
                var usuarios = await _httpClient.GetFromJsonAsync<List<UsuarioSimpleDto>>("api/usuarios") ?? new List<UsuarioSimpleDto>();
                var usuario = usuarios.FirstOrDefault(item => string.Equals(item.Email, email, StringComparison.OrdinalIgnoreCase));

                if (usuario == null)
                {
                    var nombre = email.Split('@')[0];
                    using var response = await _httpClient.PostAsJsonAsync("api/usuarios", new
                    {
                        Nombre = nombre,
                        Apellido = nombre,
                        Email = email,
                        Password = password,
                        Rol = rol,
                        Activo = true
                    });

                    if (response.IsSuccessStatusCode)
                        usuario = await response.Content.ReadFromJsonAsync<UsuarioSimpleDto>();
                }

                if (usuario != null && usuario.ResolveId() > 0)
                    HttpContext.Session.SetString("UsuarioApiId", usuario.ResolveId().ToString());
            }
            catch (HttpRequestException)
            {
                // El login local sigue funcionando si la API está temporalmente fuera de servicio.
            }
        }

        private async Task RestaurarCarritoApiAsync()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UsuarioApiId")))
                return;

            var claveCarrito = UserSessionKeys.ForUser(HttpContext.Session, "CarritoSession");
            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString(claveCarrito)))
                return;

            if (!int.TryParse(HttpContext.Session.GetString("UsuarioApiId"), out var usuarioId))
                return;

            try
            {
                var carrito = await _httpClient.GetFromJsonAsync<List<ItemCarrito>>($"api/usuarios/{usuarioId}/carrito") ?? new List<ItemCarrito>();
                HttpContext.Session.SetString(claveCarrito, JsonSerializer.Serialize(carrito));
            }
            catch (HttpRequestException)
            {
                // El carrito podrá restaurarse cuando la API vuelva a estar disponible.
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