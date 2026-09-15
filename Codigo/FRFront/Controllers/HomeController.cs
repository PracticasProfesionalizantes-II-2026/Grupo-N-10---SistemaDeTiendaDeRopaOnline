using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FRFront.Models;

namespace FRFront.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Lista centralizada de productos
        private static readonly List<Producto> _productos = new List<Producto>
        {
            new Producto
            {
                Nombre = "BUZO VCV",
                Codigo = "VCV-001",
                Precio = 100000,
                PrecioAnterior = 150000,
                EsOferta = true,
                Genero = "Hombre",
                Categoria = "Abrigos",
                Descripcion = "Este buzo está hecho para la gente elegante.",
                Imagen = "~/images/hombres2.png",
                Talles = new string[] { "S", "M", "L", "XL" },
                SinStock = false
            },
            new Producto
            {
                Nombre = "JOGGING ADDIS",
                Codigo = "JOG-002",
                Precio = 40000,
                PrecioAnterior = 50000,
                EsOferta = true,
                Genero = "Mujer",
                Categoria = "Pantalones",
                Descripcion = "Jogging urbano de algodón cómodo, diseñado con la estética streetwear del proyecto.",
                Imagen = "~/images/mujeres.png",
                Talles = new string[] { "S", "M", "L", "XL" },
                SinStock = false
            },
            new Producto
            {
                Nombre = "REMERA FIT FRIENDS",
                Codigo = "REM-003",
                Precio = 50000,
                PrecioAnterior = null,
                EsOferta = false,
                Genero = "Hombre",
                Categoria = "Remeras",
                Descripcion = "Remera de algodón premium con calce fit ideal para cualquier ocasión urbana.",
                Imagen = "~/images/hombres.png",
                Talles = new string[] { "S", "M", "L", "XL" },
                SinStock = false
            },
            new Producto
            {
                Nombre = "BUZO SEEKERS",
                Codigo = "M-BS-01",
                Precio = 78000,
                PrecioAnterior = null,
                EsOferta = false,
                Genero = "Mujer",
                Categoria = "Abrigos",
                Descripcion = "Buzo urbano de algodón rústico para mujer, diseño cómodo y moderno.",
                Imagen = "~/images/mujeres2.png",
                Talles = new string[] { "S", "M", "L" },
                SinStock = false
            },
            new Producto
            {
                Nombre = "CAMISA TRAMAS",
                Codigo = "CA-004",
                Precio = 80000,
                PrecioAnterior = null,
                EsOferta = false,
                Genero = "Hombre",
                Categoria = "Remeras",
                Descripcion = "Camisa de tejido tramado de alta calidad para la temporada verano.",
                Imagen = "~/images/hombres3.png",
                Talles = new string[] { "M", "L", "XL" },
                SinStock = false
            },
            new Producto
            {
                Nombre = "Remera Arrow",
                Codigo = "R-AR-01",
                Precio = 78000,
                PrecioAnterior = null,
                EsOferta = false,
                Genero = "Mujer",
                Categoria = "Remeras",
                Descripcion = "Remera urbana de algodón elegante para mujer, diseño cómodo y moderno.",
                Imagen = "~/images/nuevo.png",
                Talles = new string[] { "S", "M", "L" },
                SinStock = true
            }
        };

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_productos);
        }

        public IActionResult Lanzamientos()
        {
            return View(_productos);
        }

        // Acción para la sección de HOMBRE
        public IActionResult Hombre(string categoria)
        {
            var query = _productos.Where(p => p.Genero.Equals("Hombre", System.StringComparison.OrdinalIgnoreCase));
            
            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(p => p.Categoria.Equals(categoria, System.StringComparison.OrdinalIgnoreCase));
            }

            ViewData["TituloSeccion"] = "SECCIÓN HOMBRES";
            ViewData["GeneroActual"] = "Hombre";
            
            return View("Seccion", query.ToList());
        }

        // Acción para la sección de MUJER
        public IActionResult Mujer(string categoria)
        {
            var query = _productos.Where(p => p.Genero.Equals("Mujer", System.StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(p => p.Categoria.Equals(categoria, System.StringComparison.OrdinalIgnoreCase));
            }

            ViewData["TituloSeccion"] = "SECCIÓN MUJERES";
            ViewData["GeneroActual"] = "Mujer";

            return View("Seccion", query.ToList());
        }

        // Acción para ver la sección de Ofertas
        public IActionResult Ofertas()
        {
            var productosOferta = _productos.Where(p => p.EsOferta).ToList();
            return View(productosOferta);
        }

        // Acción para mostrar el Catálogo General
        public IActionResult Catalogo()
        {
            return View(_productos);
        }

        // Acción dinámica para el Detalle de un Producto
        public IActionResult DetalleProducto(string nombre)
        {
            string productoBuscado = !string.IsNullOrEmpty(nombre) ? nombre.ToUpper().Trim() : "BUZO VCV";
            
            var productoEncontrado = _productos.FirstOrDefault(p => p.Nombre.ToUpper() == productoBuscado) ?? _productos.First();

            ViewData["Nombre"] = productoEncontrado.Nombre;
            ViewData["Codigo"] = productoEncontrado.Codigo;
            ViewData["Precio"] = $"$ {productoEncontrado.Precio:N2}";
            ViewData["PrecioNumerico"] = productoEncontrado.Precio; // <--- AQUÍ SE AGREGA EL VALOR NUMÉRICO PARA EL CARRITO
            ViewData["PrecioAnterior"] = productoEncontrado.PrecioAnterior.HasValue ? $"$ {productoEncontrado.PrecioAnterior.Value:N2}" : "";
            ViewData["EsOferta"] = productoEncontrado.EsOferta;
            ViewData["Genero"] = productoEncontrado.Genero;
            ViewData["Categoria"] = productoEncontrado.Categoria;
            ViewData["Descripcion"] = productoEncontrado.Descripcion;
            ViewData["Imagen"] = productoEncontrado.Imagen;
            ViewData["Talles"] = productoEncontrado.Talles;
            ViewData["SinStock"] = productoEncontrado.SinStock;

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

        [HttpGet]
        public IActionResult CambiarContrasena()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CambiarContrasena(CambiarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public IActionResult CambiarContrasenaEmpleadoExito()
        {
            return View();
        }
    }
}