using Microsoft.AspNetCore.Mvc;

namespace FRFront.Controllers
{
    public class AdministradorController : Controller
    {
        // GET: /Administrador/
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Administrador/ConfigurarTienda
        public IActionResult ConfigurarTienda()
        {
            return View();
        }
    }
}