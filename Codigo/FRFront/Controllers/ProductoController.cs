using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using System.Text.Json;

namespace FRFront.Controllers
{
    public class ProductoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductoController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
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
                // Si la API no está disponible o falla la conexión, mostramos los datos de prueba
            }

            // Fallback de prueba con imágenes locales de wwwroot/images
            if (!productos.Any())
            {
                productos = new List<ProductoDto>
                {
                    new ProductoDto { Id = 1, Nombre = "BUZO VCV", Precio = 80000, Talles = "M/L", Color = "BEIGE", Stock = 10, Categoria = "abrigos", ImagenUrl = "/images/hombres.png", Disponible = true },
                    new ProductoDto { Id = 2, Nombre = "REMERA ACTIVE", Precio = 20000, Talles = "XS/S/M", Color = "NEGRO", Stock = 14, Categoria = "remeras", ImagenUrl = "/images/mujeres.png", Disponible = true },
                    new ProductoDto { Id = 3, Nombre = "HOODIE URBAN", Precio = 65000, Talles = "L/XL", Color = "VERDE", Stock = 8, Categoria = "abrigos", ImagenUrl = "/images/hombres2.png", Disponible = true },
                    new ProductoDto { Id = 4, Nombre = "TOP OVERSIDE", Precio = 25000, Talles = "S/M", Color = "BLANCO", Stock = 5, Categoria = "remeras", ImagenUrl = "/images/mujeres2.png", Disponible = true },
                    new ProductoDto { Id = 5, Nombre = "PANTALON CARGO", Precio = 55000, Talles = "38/40/42", Color = "NEGRO", Stock = 12, Categoria = "pantalones", ImagenUrl = "/images/hombres3.png", Disponible = true }
                };
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
                    var response = await _httpClient.PostAsync("api/productos", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Producto creado con éxito";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio Backend.");
                }
            }

            return View("~/Views/Productos/Crear.cshtml", nuevoProducto);
        }

        // GET: /Producto/Modificar/5
        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/productos/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var producto = JsonSerializer.Deserialize<ProductoDto>(content, _jsonOptions);
                    return View("~/Views/Productos/Modificar.cshtml", producto);
                }
            }
            catch
            {
                // Error de red
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Producto/Modificar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar(ProductoDto productoModificado, IFormFile? imagenFile)
        {
            if (ModelState.IsValid)
            {
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
                    var response = await _httpClient.PutAsync($"api/productos/{productoModificado.Id}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Producto modificado con éxito";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "No se pudo actualizar el producto en el Backend.");
                }
            }

            return View("~/Views/Productos/Modificar.cshtml", productoModificado);
        }

        // POST: /Producto/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/productos/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Producto eliminado correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el producto.";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Error de conexión al intentar eliminar.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}