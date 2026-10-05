using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using FRFront.Models;
using System.Text.Json;
using System.Net.Http.Json;

namespace FRFront.Controllers
{
    public class ProductoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductoController> _logger;

        // Lista estática en memoria para conservar los cambios (crear, modificar, eliminar) cuando la API no está disponible
        private static List<ProductoDto>? _productosEnMemoria;

        public ProductoController(
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment,
            ILogger<ProductoController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
            _environment = environment;
            _logger = logger;
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
            var apiDisponible = false;

            try
            {
                var response = await _httpClient.GetAsync("api/productos");
                if (response.IsSuccessStatusCode)
                {
                    apiDisponible = true;
                    var content = await response.Content.ReadAsStringAsync();
                    productos = JsonSerializer.Deserialize<List<ProductoDto>>(content, _jsonOptions) ?? new List<ProductoDto>();
                    productos = productos.Where(producto => producto.Activo).ToList();
                    foreach (var producto in productos)
                    {
                        if (string.IsNullOrWhiteSpace(producto.Color))
                        {
                            producto.Color = producto.Colores ?? string.Empty;
                        }
                    }
                }
            }
            catch
            {
                // API no disponible
            }

            if (!apiDisponible)
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
        public async Task<IActionResult> Crear()
        {
            await PrepararCategoriasAsync();
            return View("~/Views/Productos/Crear.cshtml");
        }

        // POST: /Producto/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ProductoDto nuevoProducto, IFormFile? imagenFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (imagenFile is not null && imagenFile.Length > 0)
                    {
                        nuevoProducto.ImagenUrl = await GuardarImagenAsync(imagenFile);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(imagenFile), ex.Message);
                    await PrepararCategoriasAsync();
                    return View("~/Views/Productos/Crear.cshtml", nuevoProducto);
                }

                if (string.IsNullOrEmpty(nuevoProducto.ImagenUrl))
                {
                    nuevoProducto.ImagenUrl = "/images/hombres.png";
                }

                try
                {
                    var categoria = await ObtenerCategoriaAsync(nuevoProducto.Categoria);
                    if (categoria == null)
                    {
                        categoria = await CrearCategoriaAsync(nuevoProducto.Categoria);
                    }

                    if (categoria == null)
                    {
                        ModelState.AddModelError(nameof(nuevoProducto.Categoria), "No se pudo crear la categoría en la base de datos.");
                        await PrepararCategoriasAsync();
                        return View("~/Views/Productos/Crear.cshtml", nuevoProducto);
                    }

                    var response = await _httpClient.PostAsJsonAsync("api/productos", new
                    {
                        nuevoProducto.Nombre,
                        nuevoProducto.Descripcion,
                        nuevoProducto.Precio,
                        nuevoProducto.PrecioAnterior,
                        nuevoProducto.EsOferta,
                        nuevoProducto.ImagenUrl,
                        nuevoProducto.Talles,
                        Colores = nuevoProducto.Color,
                        Stock = nuevoProducto.Stock,
                        EmpresaId = categoria.EmpresaId,
                        CategoriaId = categoria.Id
                    });

                    if (!response.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError(string.Empty, "No se pudo guardar el producto en la base de datos.");
                        await PrepararCategoriasAsync();
                        return View("~/Views/Productos/Crear.cshtml", nuevoProducto);
                    }

                    if (nuevoProducto.EsOferta)
                        await EnviarDifusionOfertaAsync(nuevoProducto);
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError(string.Empty, "No se pudo conectar con la API para guardar el producto.");
                    await PrepararCategoriasAsync();
                    return View("~/Views/Productos/Crear.cshtml", nuevoProducto);
                }

                TempData["SuccessMessage"] = "Producto creado con éxito.";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Productos/Crear.cshtml", nuevoProducto);
        }

        private async Task PrepararCategoriasAsync()
        {
            try
            {
                ViewBag.Categorias = await _httpClient.GetFromJsonAsync<List<CategoriaLookupDto>>("api/categorias")
                    ?? new List<CategoriaLookupDto>();
                ViewBag.CategoriasError = null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "No se pudieron cargar las categorías desde la API.");
                ViewBag.Categorias = new List<CategoriaLookupDto>();
                ViewBag.CategoriasError = "No se pudieron cargar las categorías. Verificá que la API esté ejecutándose.";
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "La API devolvió una respuesta inválida al cargar categorías.");
                ViewBag.Categorias = new List<CategoriaLookupDto>();
                ViewBag.CategoriasError = "La API devolvió una respuesta inválida al cargar las categorías.";
            }
        }

        private async Task<CategoriaLookupDto?> CrearCategoriaAsync(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            var empresas = await _httpClient.GetFromJsonAsync<List<EmpresaLookup>>("api/empresa");
            var empresa = empresas?.FirstOrDefault();
            if (empresa is null)
                return null;

            var response = await _httpClient.PostAsJsonAsync("api/categorias", new
            {
                Nombre = nombre.Trim(),
                EmpresaId = empresa.Id
            });

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CategoriaLookupDto>(_jsonOptions);
        }

        private async Task<string> GuardarImagenAsync(IFormFile imagenFile)
        {
            var imagesPath = Path.Combine(_environment.WebRootPath, "images");
            Directory.CreateDirectory(imagesPath);

            var extension = Path.GetExtension(imagenFile.FileName).ToLowerInvariant();
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".jfif" };
            if (!extensionesPermitidas.Contains(extension))
                throw new InvalidOperationException("El formato de imagen no está permitido.");

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(imagesPath, fileName);
            await using var stream = System.IO.File.Create(filePath);
            await imagenFile.CopyToAsync(stream);
            return $"/images/{fileName}";
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
                    if (producto != null && string.IsNullOrWhiteSpace(producto.Color))
                    {
                        producto.Color = producto.Colores ?? string.Empty;
                    }
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

            if (producto == null && id >= 10000)
            {
                producto = HomeController.ObtenerProductosLocalesDto()
                    .FirstOrDefault(p => p.Id == id);
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
                try
                {
                    if (imagenFile is not null && imagenFile.Length > 0)
                    {
                        productoModificado.ImagenUrl = await GuardarImagenAsync(imagenFile);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(imagenFile), ex.Message);
                    return View("~/Views/Productos/Modificar.cshtml", productoModificado);
                }

                var productoAnterior = await ObtenerProductoApiAsync(productoModificado.Id);

                var categoria = productoModificado.CategoriaId > 0
                    ? new CategoriaLookupDto
                    {
                        Id = productoModificado.CategoriaId,
                        EmpresaId = 0,
                        Nombre = productoModificado.Categoria
                    }
                    : await ObtenerCategoriaAsync(productoModificado.Categoria);

                if (categoria == null)
                {
                    ModelState.AddModelError(nameof(productoModificado.Categoria), "La categoría del producto no existe en la base de datos.");
                    return View("~/Views/Productos/Modificar.cshtml", productoModificado);
                }

                var request = new
                {
                    productoModificado.Nombre,
                    productoModificado.Descripcion,
                    productoModificado.Precio,
                    productoModificado.PrecioAnterior,
                    productoModificado.EsOferta,
                    ImagenUrl = productoModificado.ImagenUrl,
                    productoModificado.Talles,
                    Colores = productoModificado.Color,
                    productoModificado.Stock,
                    productoModificado.CategoriaId,
                    productoModificado.SubcategoriaId
                };

                try
                {
                    HttpResponseMessage response;
                    if (productoModificado.Id >= 10000)
                    {
                        response = await _httpClient.PostAsJsonAsync("api/productos", new
                        {
                            productoModificado.Nombre,
                            productoModificado.Descripcion,
                            productoModificado.Precio,
                            productoModificado.PrecioAnterior,
                            productoModificado.EsOferta,
                            productoModificado.ImagenUrl,
                            productoModificado.Talles,
                            Colores = productoModificado.Color,
                            Stock = productoModificado.Stock,
                            EmpresaId = categoria.EmpresaId,
                            CategoriaId = categoria.Id,
                            productoModificado.SubcategoriaId
                        });
                    }
                    else
                    {
                        response = await _httpClient.PutAsJsonAsync($"api/productos/{productoModificado.Id}", request);
                    }

                    if (response.IsSuccessStatusCode && productoModificado.EsOferta && !(productoAnterior?.EsOferta ?? false))
                    {
                        await EnviarDifusionOfertaAsync(productoModificado);
                    }
                    else if (!response.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError(string.Empty, "No se pudo guardar el producto en la base de datos.");
                        return View("~/Views/Productos/Modificar.cshtml", productoModificado);
                    }

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
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError(string.Empty, "No se pudo conectar con la API para guardar el producto.");
                    return View("~/Views/Productos/Modificar.cshtml", productoModificado);
                }

                TempData["SuccessMessage"] = "Producto modificado con éxito.";
                return RedirectToAction("Productos", "Administrador");
            }

            return View("~/Views/Productos/Modificar.cshtml", productoModificado);
        }

        private async Task<ProductoDto?> ObtenerProductoApiAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ProductoDto>($"api/productos/{id}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        private async Task<CategoriaLookupDto?> ObtenerCategoriaAsync(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            var categorias = await _httpClient.GetFromJsonAsync<List<CategoriaLookupDto>>("api/categorias");
            return categorias?.FirstOrDefault(c =>
                c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
        }

        private sealed class EmpresaLookup
        {
            public int Id { get; set; }
        }

        private async Task EnviarDifusionOfertaAsync(ProductoDto producto)
        {
            try
            {
                await _httpClient.PostAsJsonAsync("api/notificaciones/difusion", new
                {
                    Mensaje = $"¡Oferta especial! {producto.Nombre} ahora está disponible con descuento.",
                    ImagenUrl = producto.ImagenUrl
                });
            }
            catch (HttpRequestException)
            {
                // La oferta ya fue guardada; la difusión podrá reenviarse manualmente.
            }
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
                var response = await _httpClient.DeleteAsync($"api/productos/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("La API no pudo descontinuar el producto {ProductoId}. Código HTTP: {StatusCode}", id, response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "No se pudo conectar con la API para descontinuar el producto {ProductoId}.", id);
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