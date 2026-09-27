using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FRFront.Models;

namespace FRFront.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string emailLower = model.Email.ToLower();

            // 1. Obtener la contraseña guardada previamente en la sesión o la predeterminada "Admin123"
            string claveEsperada = HttpContext.Session.GetString("PasswordSesion") ?? "Admin123";

            // 2. Validar que la contraseña coincida con la activa
            if (model.Password != claveEsperada)
            {
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
                return RedirectToAction("Index", "Administrador");
            }
            else if (emailLower.Contains("empleado") || emailLower.Contains("cajero"))
            {
                HttpContext.Session.SetString("RolSesion", "Empleado");
                return RedirectToAction("Index", "Empleado");
            }
            else
            {
                bool estaBloqueado = emailLower.Contains("bloqueado");

                if (estaBloqueado)
                {
                    ModelState.AddModelError(string.Empty, "Su cuenta se encuentra BLOQUEADA. Por favor, contacte con soporte.");
                    return View(model);
                }

                HttpContext.Session.SetString("RolSesion", "Cliente");
                HttpContext.Session.SetString("EstadoCliente", "ACTIVO");
                HttpContext.Session.SetString("UltimoIngreso", DateTime.Now.ToString("o"));

                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            // Resguardar la clave personalizada antes de limpiar variables de usuario
            string? claveGuardada = HttpContext.Session.GetString("PasswordSesion");

            HttpContext.Session.Clear();

            if (!string.IsNullOrEmpty(claveGuardada))
            {
                HttpContext.Session.SetString("PasswordSesion", claveGuardada);
            }

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

            TempData["SuccessMessage"] = "Registro completado con éxito. Ya puedes iniciar sesión.";
            return RedirectToAction("Login", "Account");
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
            string claveAlmacenada = HttpContext.Session.GetString("PasswordSesion") ?? "Admin123";

            // Validar clave actual
            if (!string.IsNullOrEmpty(model.ContrasenaActual) && model.ContrasenaActual != claveAlmacenada)
            {
                ModelState.AddModelError("ContrasenaActual", "La contraseña actual ingresada es incorrecta.");
            }

            // Validar requerimientos de la nueva clave (Mayúscula y Número)
            if (!string.IsNullOrEmpty(model.NuevaContrasena))
            {
                bool tieneMayuscula = model.NuevaContrasena.Any(char.IsUpper);
                bool tieneNumero = model.NuevaContrasena.Any(char.IsDigit);

                if (!tieneMayuscula || !tieneNumero)
                {
                    ModelState.AddModelError("NuevaContrasena", "La nueva contraseña debe incluir al menos una letra mayúscula y un número.");
                }

                if (model.NuevaContrasena == model.ContrasenaActual)
                {
                    ModelState.AddModelError("NuevaContrasena", "La nueva contraseña no puede ser igual a la contraseña actual.");
                }
            }

            // Validar coincidencia entre nueva clave y confirmación
            if (!string.IsNullOrEmpty(model.NuevaContrasena) && !string.IsNullOrEmpty(model.ConfirmarContrasena))
            {
                if (model.NuevaContrasena != model.ConfirmarContrasena)
                {
                    ModelState.AddModelError("ConfirmarContrasena", "La nueva contraseña y su confirmación no coinciden.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/CambiarContrasena.cshtml", model);
            }

            // Actualizar la clave para las próximas validaciones
            HttpContext.Session.SetString("PasswordSesion", model.NuevaContrasena);

            TempData["SuccessMessage"] = "Tu contraseña ha sido actualizada con éxito.";
            return RedirectToAction("CambiarContrasena");
        }
    }
}