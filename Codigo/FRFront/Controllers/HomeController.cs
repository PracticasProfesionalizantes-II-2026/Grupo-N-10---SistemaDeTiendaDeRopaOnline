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

        // Catálogo local de respaldo. Cada imagen representa una prenda única.
        private static readonly List<Producto> _productos = new()
        {
            CrearProducto("Abrigo North", "abrigo-hombre.jfif", "Hombre", "Abrigos", "Negro", 98000),
            CrearProducto("Abrigo Urban", "abrigo-hombre-2.jfif", "Hombre", "Abrigos", "Gris", 112000, true),
            CrearProducto("Abrigo Essential", "abrigo-hombre-3.webp", "Hombre", "Abrigos", "Beige", 105000),
            CrearProducto("Buzo Street", "buzo-hombre3.webp", "Hombre", "Abrigos", "Negro", 85000),
            CrearProducto("Buzo Comfy", "buzo4-hombre.webp", "Hombre", "Abrigos", "Verde", 79000),
            CrearProducto("Buzo Classic", "buzo5-hombre.jfif", "Hombre", "Abrigos", "Azul", 82000),
            CrearProducto("Campera Line", "campera2-hombre.webp", "Hombre", "Abrigos", "Marrón", 125000),
            CrearProducto("Campera Denim", "camperahombre.jpg", "Hombre", "Abrigos", "Azul", 118000),
            CrearProducto("Remera Basic", "remera-hombre4.jfif", "Hombre", "Remeras", "Blanco", 42000),
            CrearProducto("Remera Sport", "remera7hombre.jfif", "Hombre", "Remeras", "Negro", 45000),
            CrearProducto("Remera Essential", "remerahombre6.jfif", "Hombre", "Remeras", "Verde", 47000),

            CrearProducto("Abrigo Soft", "abrigo-mujer.jfif", "Mujer", "Abrigos", "Beige", 99000),
            CrearProducto("Abrigo Cozy", "abrigo-mujer-2.jpg", "Mujer", "Abrigos", "Gris", 108000),
            CrearProducto("Abrigo Winter", "abrigo-mujer3.jfif", "Mujer", "Abrigos", "Negro", 115000, true),
            CrearProducto("Buzo Cozy", "buzo-mujer.jfif", "Mujer", "Abrigos", "Rosa", 76000),
            CrearProducto("Buzo Oversize", "buzo2-mujer.webp", "Mujer", "Abrigos", "Gris", 81000),
            CrearProducto("Campera Puffer", "campera-mujer.webp", "Mujer", "Abrigos", "Negro", 128000),
            CrearProducto("Campera Light", "campera-mujer2.webp", "Mujer", "Abrigos", "Verde", 119000),
            CrearProducto("Jean Wide", "jeanmujer.jfif", "Mujer", "Pantalones", "Azul", 72000),
            CrearProducto("Jean Straight", "jeanmujer2.webp", "Mujer", "Pantalones", "Celeste", 75000),
            CrearProducto("Jogging Urban", "joggin-mujer.jfif", "Mujer", "Pantalones", "Gris", 52000, true),
            CrearProducto("Cargo Relax", "pantalon-cargo-mujer.webp", "Mujer", "Pantalones", "Verde", 68000),
            CrearProducto("Pollera Denim", "pollera-mujer2.jfif", "Mujer", "Polleras", "Azul", 56000),
            CrearProducto("Pollera Black", "pollera-negra.jpg", "Mujer", "Polleras", "Negro", 58000),
            CrearProducto("Remera Soft", "remera-mujer2.jfif", "Mujer", "Remeras", "Blanco", 39000),
            CrearProducto("Remera Urban", "remera3-mujer.jpg", "Mujer", "Remeras", "Rojo", 43000),
            CrearProducto("Remera Basic", "remera5-mujer.jfif", "Mujer", "Remeras", "Negro", 41000),
            CrearProducto("Top Essential", "top-negro.webp", "Mujer", "Remeras", "Negro", 35000),
            CrearProducto("Vestido Daily", "vestido-mujer.jfif", "Mujer", "Vestidos", "Negro", 85000),
            CrearProducto("Vestido Flow", "vestido-mujer2.webp", "Mujer", "Vestidos", "Rojo", 92000),
            CrearProducto("Vestido Midi", "vestido-mujer3.webp", "Mujer", "Vestidos", "Verde", 95000),
            CrearProducto("Vestido Summer", "vestido-mujer4.webp", "Mujer", "Vestidos", "Blanco", 88000),
            CrearProducto("Vestido Party", "vestidomujer5.jfif", "Mujer", "Vestidos", "Azul", 102000)
        };

        public static List<ProductoDto> ObtenerProductosLocalesDto()
        {
            return _productos.Select((producto, indice) => new ProductoDto
            {
                Id = 10000 + indice,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                PrecioAnterior = producto.PrecioAnterior,
                EsOferta = producto.EsOferta,
                Talles = string.Join(",", producto.Talles),
                Color = producto.Color,
                Colores = producto.Color,
                Categoria = producto.Categoria,
                Descripcion = producto.Descripcion,
                ImagenUrl = producto.Imagen.Replace("~/", "/", StringComparison.OrdinalIgnoreCase),
                Stock = producto.SinStock ? 0 : 10,
                Disponible = !producto.SinStock
            }).ToList();
        }

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("BackendApi");
        }

        private static Producto CrearProducto(string nombre, string imagen, string genero, string categoria, string color, decimal precio, bool esOferta = false)
        {
            return new Producto
            {
                Nombre = nombre,
                Codigo = nombre.Replace(" ", "-").ToUpperInvariant(),
                Precio = esOferta ? precio * 0.8m : precio,
                PrecioAnterior = esOferta ? precio : null,
                EsOferta = esOferta,
                Genero = genero,
                Categoria = categoria,
                Color = color,
                Descripcion = $"{nombre} de colección F&R.",
                Imagen = $"~/images/{imagen}",
                Talles = new[] { "S", "M", "L", "XL" },
                TiempoEntregaDias = categoria.Equals("Abrigos", StringComparison.OrdinalIgnoreCase) ? 7 : 2
            };
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await ObtenerProductosAsync());
        }

        public async Task<IActionResult> Lanzamientos(string? categoria, string? color, int? entrega, decimal? precioMin, decimal? precioMax, string? orden)
        {
            var productos = await AplicarOrdenAsync(AplicarFiltros(await ObtenerProductosAsync(), categoria, color, entrega, precioMin, precioMax, orden), orden);
            PrepararFiltros(categoria, color, entrega, precioMin, precioMax, orden);
            return View(productos);
        }

        // Acción para la sección de HOMBRE
        public async Task<IActionResult> Hombre(string? categoria, string? color, int? entrega, decimal? precioMin, decimal? precioMax, string? orden)
        {
            var query = (await ObtenerProductosAsync()).Where(p => p.Genero.Equals("Hombre", System.StringComparison.OrdinalIgnoreCase));
            
            var productos = await AplicarOrdenAsync(AplicarFiltros(query, categoria, color, entrega, precioMin, precioMax, orden), orden);

            ViewData["TituloSeccion"] = "SECCIÓN HOMBRES";
            ViewData["GeneroActual"] = "Hombre";
            
            PrepararFiltros(categoria, color, entrega, precioMin, precioMax, orden);
            return View("Seccion", productos);
        }

        // Acción para la sección de MUJER
        public async Task<IActionResult> Mujer(string? categoria, string? color, int? entrega, decimal? precioMin, decimal? precioMax, string? orden)
        {
            var query = (await ObtenerProductosAsync()).Where(p => p.Genero.Equals("Mujer", System.StringComparison.OrdinalIgnoreCase));

            var productos = await AplicarOrdenAsync(AplicarFiltros(query, categoria, color, entrega, precioMin, precioMax, orden), orden);

            ViewData["TituloSeccion"] = "SECCIÓN MUJERES";
            ViewData["GeneroActual"] = "Mujer";

            PrepararFiltros(categoria, color, entrega, precioMin, precioMax, orden);
            return View("Seccion", productos);
        }

        // Acción para ver la sección de Ofertas
        public async Task<IActionResult> Ofertas(string? categoria, string? color, int? entrega, decimal? precioMin, decimal? precioMax, string? orden)
        {
            var productosOferta = await AplicarOrdenAsync(AplicarFiltros((await ObtenerProductosAsync()).Where(p => p.EsOferta), categoria, color, entrega, precioMin, precioMax, orden), orden);
            PrepararFiltros(categoria, color, entrega, precioMin, precioMax, orden);
            return View(productosOferta);
        }

        // Acción para mostrar el Catálogo General
        public async Task<IActionResult> Catalogo(string? busqueda, string? categoria, string? color, int? entrega, decimal? precioMin, decimal? precioMax, string? orden)
        {
            var productos = (await ObtenerProductosAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                productos = productos.Where(producto => producto.Nombre.Contains(busqueda.Trim(), System.StringComparison.OrdinalIgnoreCase));
            }

            var filtrados = await AplicarOrdenAsync(AplicarFiltros(productos, categoria, color, entrega, precioMin, precioMax, orden), orden);
            ViewData["Busqueda"] = busqueda;
            PrepararFiltros(categoria, color, entrega, precioMin, precioMax, orden);
            return View(filtrados);
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
                    var productos = productosApi.Select(producto => new Producto
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
                        Color = string.IsNullOrWhiteSpace(producto.Colores) ? producto.Color : producto.Colores,
                        Genero = DeterminarGenero(producto),
                        EsOferta = producto.EsOferta,
                        TiempoEntregaDias = producto.Categoria.Contains("abrigo", StringComparison.OrdinalIgnoreCase) ? 7 : 2
                    }).ToList();

                    AgregarProductosLocalesSinRepetir(productos);

                    return productos;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "No se pudo consultar el catálogo persistido en FYR-API.");
            }

            return _productos;
        }

        private static string DeterminarGenero(ProductoDto producto)
        {
            var texto = string.Join(" ", producto.Nombre, producto.Categoria, producto.Colores, producto.ImagenUrl);
            if (texto.Contains("mujer", StringComparison.OrdinalIgnoreCase) || texto.Contains("dama", StringComparison.OrdinalIgnoreCase))
                return "Mujer";
            if (texto.Contains("hombre", StringComparison.OrdinalIgnoreCase) || texto.Contains("caballero", StringComparison.OrdinalIgnoreCase))
                return "Hombre";

            return string.Empty;
        }

        private static void AgregarProductosLocalesSinRepetir(List<Producto> productos)
        {
            foreach (var productoLocal in _productos)
            {
                var imagenRepetida = productos.Any(actual => NormalizarImagen(actual.Imagen).Equals(NormalizarImagen(productoLocal.Imagen), StringComparison.OrdinalIgnoreCase));
                if (!imagenRepetida)
                    AgregarProductoRespaldo(productos, productoLocal);
            }
        }

        private static string NormalizarImagen(string? imagen)
        {
            return (imagen ?? string.Empty).Replace("~/", "/", StringComparison.OrdinalIgnoreCase).Trim();
        }

        private static void AgregarProductoRespaldo(List<Producto> productos, Producto respaldo)
        {
            productos.Add(new Producto
            {
                Id = respaldo.Id,
                Nombre = respaldo.Nombre,
                Codigo = respaldo.Codigo,
                Precio = respaldo.Precio,
                PrecioAnterior = respaldo.PrecioAnterior,
                EsOferta = respaldo.EsOferta,
                Genero = respaldo.Genero,
                Categoria = respaldo.Categoria,
                Descripcion = respaldo.Descripcion,
                Imagen = respaldo.Imagen,
                Talles = respaldo.Talles,
                Color = respaldo.Color,
                TiempoEntregaDias = respaldo.TiempoEntregaDias,
                SinStock = respaldo.SinStock
            });
        }

        private static List<Producto> AplicarFiltros(IEnumerable<Producto> productos, string? categoria, string? color, int? entrega, decimal? precioMin, decimal? precioMax, string? orden)
        {
            var resultado = productos;
            if (!string.IsNullOrWhiteSpace(categoria))
            {
                if (categoria.Equals("Buzos", StringComparison.OrdinalIgnoreCase))
                    resultado = resultado.Where(p => p.Nombre.Contains("buzo", StringComparison.OrdinalIgnoreCase));
                else if (categoria.Equals("Camperas", StringComparison.OrdinalIgnoreCase))
                    resultado = resultado.Where(p => p.Nombre.Contains("campera", StringComparison.OrdinalIgnoreCase));
                else
                    resultado = resultado.Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(color))
                resultado = resultado.Where(p => p.Color.Split(',', ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Any(colorProducto => colorProducto.Equals(color.Trim(), StringComparison.OrdinalIgnoreCase)));
            if (entrega.HasValue)
                resultado = resultado.Where(p => p.TiempoEntregaDias <= entrega.Value);
            if (precioMin.HasValue)
                resultado = resultado.Where(p => p.Precio >= precioMin.Value);
            if (precioMax.HasValue)
                resultado = resultado.Where(p => p.Precio <= precioMax.Value);

            return orden?.ToLowerInvariant() switch
            {
                _ => resultado.ToList()
            };
        }

        private async Task<List<Producto>> AplicarOrdenAsync(List<Producto> productos, string? orden)
        {
            if (!string.Equals(orden, "masComprados", StringComparison.OrdinalIgnoreCase))
            {
                return orden?.ToLowerInvariant() switch
                {
                    "precioasc" => productos.OrderBy(p => p.Precio).ToList(),
                    "preciodesc" => productos.OrderByDescending(p => p.Precio).ToList(),
                    _ => productos
                };
            }

            var cantidadesVendidas = new Dictionary<int, int>();
            try
            {
                var pedidos = await _httpClient.GetFromJsonAsync<List<PedidoDto>>("api/pedidos") ?? new List<PedidoDto>();
                foreach (var pedido in pedidos)
                {
                    var pedidoId = pedido.Id > 0 ? pedido.Id : pedido.IdPedido;
                    if (pedidoId <= 0)
                        continue;

                    var detalles = await _httpClient.GetFromJsonAsync<List<DetallePedidoDto>>($"api/pedidos/{pedidoId}/detalles") ?? new List<DetallePedidoDto>();
                    foreach (var detalle in detalles)
                    {
                        cantidadesVendidas[detalle.ProductoId] = cantidadesVendidas.GetValueOrDefault(detalle.ProductoId) + detalle.Cantidad;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "No se pudo consultar el historial de ventas para ordenar los productos.");
            }

            return productos
                .OrderByDescending(producto => cantidadesVendidas.GetValueOrDefault(producto.Id))
                .ThenBy(producto => producto.Nombre)
                .ToList();
        }

        private void PrepararFiltros(string? categoria, string? color, int? entrega, decimal? precioMin, decimal? precioMax, string? orden)
        {
            ViewData["Categoria"] = categoria;
            ViewData["Color"] = color;
            ViewData["Entrega"] = entrega;
            ViewData["PrecioMin"] = precioMin;
            ViewData["PrecioMax"] = precioMax;
            ViewData["Orden"] = orden;
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