using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FRFront.Models;

namespace FRFront.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Guardamos el correo en la sesión activa
            HttpContext.Session.SetString("UsuarioSesion", model.Email);

            string emailLower = model.Email.ToLower();

            // Evaluación de Roles para Pruebas Frontend:
            if (emailLower.Contains("admin"))
            {
                HttpContext.Session.SetString("RolSesion", "Administrador");
            }
            else if (emailLower.Contains("empleado") || emailLower.Contains("cajero"))
            {
                HttpContext.Session.SetString("RolSesion", "Empleado");
            }
            else
            {
                HttpContext.Session.SetString("RolSesion", "Cliente");
            }

            // Redirige al inicio
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

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