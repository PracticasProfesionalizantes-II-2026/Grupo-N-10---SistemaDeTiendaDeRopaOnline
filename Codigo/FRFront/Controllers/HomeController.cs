using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FRFront.Models;

namespace FRFront.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // GET: Muestra la pantalla de cambio de contraseña
        [HttpGet]
        public IActionResult CambiarContrasena()
        {
            return View();
        }

        // POST: Procesa el formulario con las validaciones
        [HttpPost]
        public IActionResult CambiarContrasena(CambiarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Si hay un error de validación, vuelve a cargar la vista con los mensajes en rojo
                return View(model);
            }

            // TODO: Aquí va la llamada a tu API/Backend para actualizar la clave

            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult CambiarContrasenaEmpleadoExito()
       {
            return View();
       }
    }
}