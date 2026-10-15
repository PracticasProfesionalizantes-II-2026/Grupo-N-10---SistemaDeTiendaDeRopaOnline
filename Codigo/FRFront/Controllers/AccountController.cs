using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using FRFront.Helpers;
using System.Net.Http.Json;
using System.Security.Cryptography;

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
            ViewData["LoginLockSeconds"] = ObtenerSegundosBloqueo(null);
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

            var email = model.Email.Trim().ToLowerInvariant();
            var claveIntentos = ObtenerClaveIntentos(email);
            var estado = ObtenerEstadoLogin(claveIntentos);
            if (estado.BloqueadoPermanentemente)
            {
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginLockSeconds"] = 0;
                ViewData["LoginRecoveryRequired"] = true;
                ModelState.AddModelError(string.Empty, "Se alcanzó el límite de intentos. Se envió un correo de recuperación de contraseña.");
                return View(model);
            }

            if (estado.SegundosBloqueo > 0)
            {
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginLockSeconds"] = estado.SegundosBloqueo;
                ModelState.AddModelError(string.Empty, $"Debe esperar {estado.SegundosBloqueo} segundos antes de volver a intentarlo.");
                return View(model);
            }

            LoginApiResponse? login;
            try
            {
                using var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
                {
                    Email = email,
                    Password = model.Password
                });

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var intentos = IncrementarIntentos(claveIntentos);
                    ViewData["ReturnUrl"] = returnUrl;
                    if (intentos >= 4)
                    {
                        HttpContext.Session.SetString($"{claveIntentos}:recuperacion", "true");
                        ViewData["LoginRecoveryRequired"] = true;
                        ModelState.AddModelError(string.Empty, "Se alcanzó el límite de intentos. Se envió un correo de recuperación de contraseña.");
                    }
                    else if (intentos == 3)
                    {
                        HttpContext.Session.SetString($"{claveIntentos}:bloqueadoHasta", DateTimeOffset.UtcNow.AddSeconds(30).ToUnixTimeSeconds().ToString());
                        ViewData["LoginLockSeconds"] = 30;
                        ModelState.AddModelError(string.Empty, "Falló el tercer intento. Debe esperar 30 segundos antes de volver a intentarlo.");
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "La contraseña ingresada es incorrecta. Intente nuevamente.");
                    }
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
            LimpiarIntentos(claveIntentos);
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

            if (string.Equals(login.TipoUsuario, "Administrador", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Administrador");

            if (string.Equals(login.TipoUsuario, "Empleado", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(login.TipoUsuario, "CAJERO", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Empleado");

            return RedirectToAction("Index", "Home");
        }

        private string ObtenerClaveIntentos(string email)
        {
            var hash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(email)));
            return $"LoginIntentos:{hash}";
        }

        private int IncrementarIntentos(string clave)
        {
            var intentos = ObtenerEnteroSesion($"{clave}:cantidad") + 1;
            HttpContext.Session.SetString($"{clave}:cantidad", intentos.ToString());
            return intentos;
        }

        private (int SegundosBloqueo, bool BloqueadoPermanentemente) ObtenerEstadoLogin(string clave)
        {
            if (HttpContext.Session.GetString($"{clave}:recuperacion") == "true")
                return (0, true);

            var bloqueoHasta = ObtenerEnteroLargoSesion($"{clave}:bloqueadoHasta");
            var segundos = Math.Max(0, (int)(bloqueoHasta - DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            if (segundos == 0 && bloqueoHasta > 0)
                HttpContext.Session.Remove($"{clave}:bloqueadoHasta");

            return (segundos, false);
        }

        private int ObtenerSegundosBloqueo(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return 0;
            return ObtenerEstadoLogin(ObtenerClaveIntentos(email.Trim().ToLowerInvariant())).SegundosBloqueo;
        }

        private int ObtenerEnteroSesion(string clave)
            => int.TryParse(HttpContext.Session.GetString(clave), out var valor) ? valor : 0;

        private long ObtenerEnteroLargoSesion(string clave)
            => long.TryParse(HttpContext.Session.GetString(clave), out var valor) ? valor : 0;

        private void LimpiarIntentos(string clave)
        {
            HttpContext.Session.Remove($"{clave}:cantidad");
            HttpContext.Session.Remove($"{clave}:bloqueadoHasta");
            HttpContext.Session.Remove($"{clave}:recuperacion");
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