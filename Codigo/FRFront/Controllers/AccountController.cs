using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FRFront.Models;

namespace FRFront.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Guardamos el correo en la sesión activa
            HttpContext.Session.SetString("UsuarioSesion", model.Email);

            // Validamos si el usuario ingresado corresponde a un Administrador
            if (model.Email.ToLower().Contains("admin"))
            {
                HttpContext.Session.SetString("RolSesion", "Administrador");
            }
            else
            {
                HttpContext.Session.SetString("RolSesion", "Cliente");
            }

            // Redirige a la pantalla principal del cliente/tienda
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            // Limpia la sesión y redirige al inicio
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Próximamente: Integración con la API/Backend
            return RedirectToAction("Login", "Account");
        }

        // GET: Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ViewBag.MensajeExito = "Si el correo ingresado coincide con una cuenta registrada, recibirás un enlace de restablecimiento.";
            return View();
        }
    }
}