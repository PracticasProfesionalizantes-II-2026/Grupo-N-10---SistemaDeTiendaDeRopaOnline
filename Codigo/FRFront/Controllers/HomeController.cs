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

        public IActionResult Hombre()
        {
            return View();
        }

        public IActionResult Mujer()
        {
            return View();
        }

        // Acción para ver el Carrito de Compras
        public IActionResult Carrito()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        // Acción para mostrar el Catálogo General
        public IActionResult Catalogo()
        {
            return View();
        }

        // Acción dinámica para el Detalle de un Producto con control de Stock y Talles específicos
        public IActionResult DetalleProducto(string nombre)
        {
            string productoBuscado = !string.IsNullOrEmpty(nombre) ? nombre.ToUpper().Trim() : "BUZO VCV";

            // Por defecto asumimos que hay stock y definimos talles estándar
            ViewData["SinStock"] = false;
            ViewData["Talles"] = new string[] { "S", "M", "L", "XL" };

            if (productoBuscado.Contains("BUZO SEEKERS"))
            {
                ViewData["Nombre"] = "BUZO SEEKERS";
                ViewData["Codigo"] = "M-BS-01";
                ViewData["Precio"] = "$ 78.000,00";
                ViewData["Descripcion"] = "Buzo urbano de algodón rústico para mujer, diseño cómodo y moderno.";
                ViewData["Imagen"] = "~/images/mujeres2.png";
                ViewData["Talles"] = new string[] { "S", "M", "L" }; // Talles específicos
            }
            else if (productoBuscado.Contains("JOGGING ADDIS"))
            {
                ViewData["Nombre"] = "JOGGING ADDIS";
                ViewData["Codigo"] = "JOG-002";
                ViewData["Precio"] = "$ 65.000,00";
                ViewData["Descripcion"] = "Jogging urbano de algodón cómodo, diseñado con la estética streetwear del proyecto.";
                ViewData["Imagen"] = "~/images/mujeres.png";
                ViewData["Talles"] = new string[] { "S", "M", "L", "XL" };
            }
            else if (productoBuscado.Contains("REMERA ACTIVE"))
            {
                ViewData["Nombre"] = "REMERA ACTIVE";
                ViewData["Codigo"] = "M-RA-03";
                ViewData["Precio"] = "$ 35.000,00";
                ViewData["Descripcion"] = "Remera deportiva y casual de mujer, tela liviana y respirable.";
                ViewData["Imagen"] = "~/images/hombres3.png";
                ViewData["SinStock"] = true; // Producto sin stock de prueba
                ViewData["Talles"] = new string[] { "XS", "S", "M" };
            }
            else if (productoBuscado.Contains("SWEATER PHOBOS"))
            {
                ViewData["Nombre"] = "SWEATER PHOBOS";
                ViewData["Codigo"] = "M-SP-04";
                ViewData["Precio"] = "$ 72.000,00";
                ViewData["Descripcion"] = "Sweater tejido fino de gran calidad para mujer, ideal para media estación.";
                ViewData["Imagen"] = "~/images/mujeres2.png";
                ViewData["Talles"] = new string[] { "S", "M", "L" };
            }
            else if (productoBuscado.Contains("PANTALON CARGO"))
            {
                ViewData["Nombre"] = "PANTALON CARGO";
                ViewData["Codigo"] = "H-PC-01";
                ViewData["Precio"] = "$ 85.000,00";
                ViewData["Descripcion"] = "Pantalón cargo de hombre con múltiples bolsillos y excelente calce urbano.";
                ViewData["Imagen"] = "~/images/hombres2.png";
                ViewData["Talles"] = new string[] { "38", "40", "42", "44" }; // Ejemplo de talles numéricos para pantalón
            }
            else if (productoBuscado.Contains("REMERA FIT FRIENDS"))
            {
                ViewData["Nombre"] = "REMERA FIT FRIENDS";
                ViewData["Codigo"] = "REM-003";
                ViewData["Precio"] = "$ 35.000,00";
                ViewData["Descripcion"] = "Remera de algodón premium con calce fit ideal para cualquier ocasión urbana.";
                ViewData["Imagen"] = "~/images/hombres3.png";
                ViewData["Talles"] = new string[] { "S", "M", "L", "XL" };
            }
            else if (productoBuscado.Contains("SWEATER TRAMAS"))
            {
                ViewData["Nombre"] = "SWEATER TRAMAS";
                ViewData["Codigo"] = "SW-004";
                ViewData["Precio"] = "$ 72.000,00";
                ViewData["Descripcion"] = "Sweater de tejido tramado de alta calidad para la temporada otoño-invierno.";
                ViewData["Imagen"] = "~/images/hombres.png";
                ViewData["Talles"] = new string[] { "M", "L", "XL" };
            }
            else if (productoBuscado.Contains("CAMISA LINO"))
            {
                ViewData["Nombre"] = "CAMISA LINO";
                ViewData["Codigo"] = "CAM-005";
                ViewData["Precio"] = "$ 55.000,00";
                ViewData["Descripcion"] = "Camisa de lino fresca y liviana, corte oversize con botones frontales.";
                ViewData["Imagen"] = "~/images/hombres2.png";
                ViewData["Talles"] = new string[] { "S", "M", "L", "XL" };
            }
            else
            {
                // Por defecto muestra el Buzo VCV
                ViewData["Nombre"] = "BUZO VCV";
                ViewData["Codigo"] = "VCV-001";
                ViewData["Precio"] = "$ 80.000,00";
                ViewData["Descripcion"] = "Este buzo está hecho para la gente elegante.";
                ViewData["Imagen"] = "~/images/hombres.png";
                ViewData["Talles"] = new string[] { "S", "M", "L", "XL" };
            }

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