using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using System.Text;
using System.Text.Json;

namespace FRFront.Controllers
{
    public partial class AdministradorController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public AdministradorController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // ==========================================
        // VISTA PRINCIPAL PANEL ADMINISTRADOR (Faltaba este método)
        // GET: /Administrador o /Administrador/Index
        // ==========================================
        [HttpGet]
        public IActionResult Index()
        {
            return View(); // Retorna Views/Administrador/Index.cshtml
        }

        // ==========================================
        // GESTIÓN DE PRODUCTOS
        // ==========================================

        // GET: Administrador/Productos
        [HttpGet]
        public async Task<IActionResult> Productos(string? categoria)
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
                // Si la API falla o no está iniciada, carga la vista con la lista vacía
            }

            if (!string.IsNullOrEmpty(categoria) && categoria != "Todos")
            {
                productos = productos.Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.CategoriaSeleccionada = categoria ?? "Todos";
            
            // Retorna la vista en Views/Productos/Index.cshtml
            return View("~/Views/Productos/Index.cshtml", productos);
        }

        // GET: Administrador/CrearProducto
        [HttpGet]
        public IActionResult CrearProducto()
        {
            return View("~/Views/Productos/Crear.cshtml");
        }

        // POST: Administrador/CrearProducto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearProducto(ProductoDto nuevoProducto, IFormFile? imagenFile)
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
                        return RedirectToAction("Productos", "Administrador");
                    }

                    ModelState.AddModelError(string.Empty, "Error al guardar el producto en el Backend.");
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "No se pudo establecer conexión con el servidor Backend.");
                }
            }

            return View("~/Views/Productos/Crear.cshtml", nuevoProducto);
        }

        // GET: Administrador/EditarProducto/1
        [HttpGet]
        public async Task<IActionResult> EditarProducto(int id)
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
                // Error de conexión con la API
            }

            return RedirectToAction("Productos", "Administrador");
        }

        // POST: Administrador/EditarProducto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProducto(ProductoDto productoModificado, IFormFile? imagenFile)
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
                        TempData["SuccessMessage"] = "Producto modificado correctamente";
                        return RedirectToAction("Productos", "Administrador");
                    }

                    ModelState.AddModelError(string.Empty, "Error al actualizar el producto en el Backend.");
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Error de comunicación con el servicio Backend.");
                }
            }

            return View("~/Views/Productos/Modificar.cshtml", productoModificado);
        }

        // POST: Administrador/EliminarProducto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/productos/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Producto eliminado correctamente";
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

            return RedirectToAction("Productos", "Administrador");
        }
    }
}