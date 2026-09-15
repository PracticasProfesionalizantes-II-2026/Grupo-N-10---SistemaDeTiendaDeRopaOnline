using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FRFront.Controllers
{
    public class EmpleadoController : Controller
    {
        // GET: /Empleado
        [HttpGet]
        public IActionResult Index()
        {
            // Validar que el usuario sea Empleado / Cajero
            string? rol = HttpContext.Session.GetString("RolSesion");

            if (string.IsNullOrEmpty(rol) || (rol != "Empleado" && rol != "Administrador"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View("~/Views/Empleado/Index.cshtml");
        }
    }
}