using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FRFront.Models;
using System.Net.Http.Json;

namespace FRFront.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

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

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("BackendApi");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await ObtenerProductosAsync());
        }

        public IActionResult Lanzamientos()
        {
            // Puedes retornar una lista de productos destacados o nuevos aquí si lo deseas
            return View();
        }

        // Acción para la sección de HOMBRE
        public async Task<IActionResult> Hombre(string categoria)
        {
            var query = (await ObtenerProductosAsync()).Where(p => p.Genero.Equals("Hombre", System.StringComparison.OrdinalIgnoreCase));
            
            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(p => p.Categoria.Equals(categoria, System.StringComparison.OrdinalIgnoreCase));
            }

            ViewData["TituloSeccion"] = "SECCIÓN HOMBRES";
            ViewData["GeneroActual"] = "Hombre";
            
            return View("Seccion", query.ToList());
        }

        // Acción para la sección de MUJER
        public async Task<IActionResult> Mujer(string categoria)
        {
            var query = (await ObtenerProductosAsync()).Where(p => p.Genero.Equals("Mujer", System.StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(p => p.Categoria.Equals(categoria, System.StringComparison.OrdinalIgnoreCase));
            }

            ViewData["TituloSeccion"] = "SECCIÓN MUJERES";
            ViewData["GeneroActual"] = "Mujer";

            return View("Seccion", query.ToList());
        }

        // Acción para ver la sección de Ofertas
        public async Task<IActionResult> Ofertas()
        {
            var productosOferta = (await ObtenerProductosAsync()).Where(p => p.EsOferta).ToList();
            return View(productosOferta);
        }

        // Acción para mostrar el Catálogo General
        public async Task<IActionResult> Catalogo(string busqueda)
        {
            var productos = (await ObtenerProductosAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                productos = productos.Where(producto => producto.Nombre.Contains(busqueda.Trim(), System.StringComparison.OrdinalIgnoreCase));
            }

            ViewData["Busqueda"] = busqueda;
            return View(productos.ToList());
        }

        // Acción dinámica para el Detalle de un Producto
        public async Task<IActionResult> DetalleProducto(string nombre)
        {
            string productoBuscado = !string.IsNullOrEmpty(nombre) ? nombre.ToUpper().Trim() : "BUZO VCV";
            
            var productos = await ObtenerProductosAsync();
            var productoEncontrado = productos.FirstOrDefault(p => p.Nombre.ToUpper() == productoBuscado) ?? productos.FirstOrDefault();

            if (productoEncontrado == null)
            {
                return NotFound();
            }

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

        private async Task<List<Producto>> ObtenerProductosAsync()
        {
            try
            {
                var productosApi = await _httpClient.GetFromJsonAsync<List<ProductoDto>>("api/productos") ?? new List<ProductoDto>();
                if (productosApi.Count > 0)
                {
                    return productosApi.Select(producto => new Producto
                    {
                        Id = producto.Id,
                        Nombre = producto.Nombre,
                        Precio = producto.Precio,
                        Descripcion = producto.Descripcion ?? string.Empty,
                        Imagen = string.IsNullOrWhiteSpace(producto.ImagenUrl) ? "~/images/logo-fr.png" : producto.ImagenUrl,
                        Categoria = producto.Categoria,
                        Talles = string.IsNullOrWhiteSpace(producto.Talles)
                            ? Array.Empty<string>()
                            : producto.Talles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                        Genero = producto.Categoria.Contains("mujer", StringComparison.OrdinalIgnoreCase) ? "Mujer" : "Hombre",
                        EsOferta = false
                    }).ToList();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "No se pudo consultar el catálogo persistido en FYR-API.");
            }

            return _productos;
        }

        [HttpGet]
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