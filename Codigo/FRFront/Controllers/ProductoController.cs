using Microsoft.AspNetCore.Mvc;

namespace FRFront.Controllers
{
    public class ProductoController : Controller
    {
        // GET: /Producto/
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Producto/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // GET: /Producto/Modificar/5
        public IActionResult Modificar(int id)
        {
            return View();
        }
    }
}