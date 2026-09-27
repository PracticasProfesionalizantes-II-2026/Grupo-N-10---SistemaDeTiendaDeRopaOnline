using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using System.Text.Json;

namespace FRFront.Controllers
{
    public class ProductoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        // Lista estática en memoria para conservar los cambios (crear, modificar, eliminar) cuando la API no está disponible
        private static List<ProductoDto>? _productosEnMemoria;

        public ProductoController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (_productosEnMemoria == null)
            {
                _productosEnMemoria = GetProductosFallback();
            }
        }

        // GET: /Producto/
        [HttpGet]
        public async Task<IActionResult> Index(string? categoria, string? busqueda)
        {
            var productos = new List<ProductoDto>();

            try
            {
                var response = await _httpClient.GetAsync("api/productos");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    productos = JsonSerializer.Deserialize<List<ProductoDto>>(content, _jsonOptions) ?? new List<ProductoDto>();
                }
            }
            catch
            {
                // API no disponible
            }

            // Si la API falla o no devuelve datos, utilizamos el estado actual en memoria local
            if (!productos.Any())
            {
                productos = _productosEnMemoria!;
            }

            // Aplicar Filtro de Categoría
            if (!string.IsNullOrEmpty(categoria) && !categoria.Equals("Todos", StringComparison.OrdinalIgnoreCase))
            {
                productos = productos.Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Aplicar Filtro de Búsqueda
            if (!string.IsNullOrEmpty(busqueda))
            {
                productos = productos.Where(p => p.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                                                 p.Talles.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                                                 p.Color.Contains(busqueda, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.CategoriaSeleccionada = categoria ?? "Todos";
            ViewBag.BusquedaActual = busqueda ?? "";

            return View("~/Views/Productos/Index.cshtml", productos);
        }

        // GET: /Producto/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View("~/Views/Productos/Crear.cshtml");
        }

        // POST: /Producto/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ProductoDto nuevoProducto, IFormFile? imagenFile)
        {
            if (ModelState.IsValid)
            {
                // Guardado local
                nuevoProducto.Id = _productosEnMemoria!.Any() ? _productosEnMemoria!.Max(p => p.Id) + 1 : 1;
                nuevoProducto.Disponible = true;
                if (string.IsNullOrEmpty(nuevoProducto.ImagenUrl))
                {
                    nuevoProducto.ImagenUrl = "/images/hombres.png";
                }

                _productosEnMemoria!.Add(nuevoProducto);

                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(nuevoProducto.Nombre ?? ""), nameof(nuevoProducto.Nombre));
                content.Add(new StringContent(nuevoProducto.Precio.ToString()), nameof(nuevoProducto.Precio));
                content.Add(new StringContent(nuevoProducto.Talles ?? ""), nameof(nuevoProducto.Talles));
                content.Add(new StringContent(nuevoProducto.Color ?? ""), nameof(nuevoProducto.Color));
                content.Add(new StringContent(nuevoProducto.Stock.ToString()), nameof(nuevoProducto.Stock));
                content.Add(new StringContent(nuevoProducto.Categoria ?? ""), nameof(nuevoProducto.Categoria));
                content.Add(new StringContent(nuevoProducto.Descripcion ?? ""), nameof(nuevoProducto.Descripcion));

                if (imagenFile != null && imagenFile.Length > 0)
                {
                    var streamContent = new StreamContent(imagenFile.OpenReadStream());
                    content.Add(streamContent, "imagenFile", imagenFile.FileName);
                }

                try
                {
                    await _httpClient.PostAsync("api/productos", content);
                }
                catch
                {
                    // Manejo local
                }

                TempData["SuccessMessage"] = "Producto creado con éxito.";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Productos/Crear.cshtml", nuevoProducto);
        }

        // GET: /Producto/Modificar/5
        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            ProductoDto? producto = null;

            try
            {
                var response = await _httpClient.GetAsync($"api/productos/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    producto = JsonSerializer.Deserialize<ProductoDto>(content, _jsonOptions);
                }
            }
            catch
            {
                // Error de red
            }

            if (producto == null)
            {
                producto = _productosEnMemoria!.FirstOrDefault(p => p.Id == id);
            }

            if (producto == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Productos/Modificar.cshtml", producto);
        }

        // POST: /Producto/Modificar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar(ProductoDto productoModificado, IFormFile? imagenFile)
        {
            if (ModelState.IsValid)
            {
                // Actualización local
                var productoLocal = _productosEnMemoria!.FirstOrDefault(p => p.Id == productoModificado.Id);
                if (productoLocal != null)
                {
                    productoLocal.Nombre = productoModificado.Nombre;
                    productoLocal.Precio = productoModificado.Precio;
                    productoLocal.Talles = productoModificado.Talles;
                    productoLocal.Color = productoModificado.Color;
                    productoLocal.Stock = productoModificado.Stock;
                    productoLocal.Categoria = productoModificado.Categoria;
                    productoLocal.Descripcion = productoModificado.Descripcion;
                    productoLocal.Disponible = productoModificado.Disponible;
                }

                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(productoModificado.Id.ToString()), nameof(productoModificado.Id));
                content.Add(new StringContent(productoModificado.Nombre ?? ""), nameof(productoModificado.Nombre));
                content.Add(new StringContent(productoModificado.Precio.ToString()), nameof(productoModificado.Precio));
                content.Add(new StringContent(productoModificado.Talles ?? ""), nameof(productoModificado.Talles));
                content.Add(new StringContent(productoModificado.Color ?? ""), nameof(productoModificado.Color));
                content.Add(new StringContent(productoModificado.Stock.ToString()), nameof(productoModificado.Stock));
                content.Add(new StringContent(productoModificado.Categoria ?? ""), nameof(productoModificado.Categoria));
                content.Add(new StringContent(productoModificado.Descripcion ?? ""), nameof(productoModificado.Descripcion));
                content.Add(new StringContent(productoModificado.Disponible.ToString()), nameof(productoModificado.Disponible));

                if (imagenFile != null && imagenFile.Length > 0)
                {
                    var streamContent = new StreamContent(imagenFile.OpenReadStream());
                    content.Add(streamContent, "imagenFile", imagenFile.FileName);
                }

                try
                {
                    await _httpClient.PutAsync($"api/productos/{productoModificado.Id}", content);
                }
                catch
                {
                    // Fallback
                }

                TempData["SuccessMessage"] = "Producto modificado con éxito.";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Productos/Modificar.cshtml", productoModificado);
        }

        // POST: /Producto/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            // 1. Borrado garantizado en lista local en memoria
            var productoLocal = _productosEnMemoria!.FirstOrDefault(p => p.Id == id);
            if (productoLocal != null)
            {
                _productosEnMemoria!.Remove(productoLocal);
            }

            // 2. Intento de borrado en API Backend
            try
            {
                await _httpClient.DeleteAsync($"api/productos/{id}");
            }
            catch
            {
                // Continuación con borrado local
            }

            TempData["SuccessMessage"] = "Producto eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Producto/EliminarProducto/5 (Alias por compatibilidad)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            return await Eliminar(id);
        }

        // DATOS DE RESPALDO (FALLBACK)
        private static List<ProductoDto> GetProductosFallback()
        {
            return new List<ProductoDto>
            {
                new ProductoDto { Id = 1, Nombre = "BUZO VCV", Precio = 80000, Talles = "M/L", Color = "BEIGE", Stock = 10, Categoria = "abrigos", ImagenUrl = "/images/hombres.png", Disponible = true },
                new ProductoDto { Id = 2, Nombre = "REMERA ACTIVE", Precio = 20000, Talles = "XS/S/M", Color = "NEGRO", Stock = 14, Categoria = "remeras", ImagenUrl = "/images/mujeres.png", Disponible = true },
                new ProductoDto { Id = 3, Nombre = "HOODIE URBAN", Precio = 65000, Talles = "L/XL", Color = "VERDE", Stock = 8, Categoria = "abrigos", ImagenUrl = "/images/hombres2.png", Disponible = true },
                new ProductoDto { Id = 4, Nombre = "TOP OVERSIDE", Precio = 25000, Talles = "S/M", Color = "BLANCO", Stock = 5, Categoria = "remeras", ImagenUrl = "/images/mujeres2.png", Disponible = true },
                new ProductoDto { Id = 5, Nombre = "PANTALON CARGO", Precio = 55000, Talles = "38/40/42", Color = "NEGRO", Stock = 12, Categoria = "pantalones", ImagenUrl = "/images/hombres3.png", Disponible = true }
            };
        }
    }
}