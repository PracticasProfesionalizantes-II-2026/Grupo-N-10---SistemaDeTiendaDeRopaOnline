using Microsoft.AspNetCore.Mvc;

namespace FRFront.Controllers
{
    public class EmpleadoController : Controller
    {
        // GET: /Empleado/
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Empleado/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }
    }
}