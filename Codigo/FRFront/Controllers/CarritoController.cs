using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using FRFront.Helpers;
using System.Text.Json;
using System.Net.Http.Json;

namespace FRFront.Controllers
{
    public class CarritoController : Controller
    {
        private readonly HttpClient _httpClient;

        public CarritoController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
        }

        public async Task<IActionResult> Index()
        {
            var carrito = await ObtenerCarritoAsync();
            return View(carrito);
        }

        // Agregar al carrito (Funciona sin estar logueado, ahora incluye el talle)
        [HttpPost]
        public async Task<IActionResult> Agregar(int productoId, string nombre, decimal precio, string imagen, string talle, string color, bool modoCompra = false, int cantidad = 1)
        {
            var talleNormalizado = string.IsNullOrWhiteSpace(talle) ? "Único" : talle.Trim();
            var colorNormalizado = string.IsNullOrWhiteSpace(color) ? "Único" : color.Trim();
            if (productoId > 0)
            {
                try
                {
                    var producto = await _httpClient.GetFromJsonAsync<ProductoDto>($"api/productos/{productoId}");
                    if (producto is not null && (!producto.Activo || producto.Stock <= 0))
                    {
                        TempData["ErrorCarrito"] = "El producto no tiene stock disponible.";
                        return RedirectToAction("Index", "Home");
                    }

                    if (modoCompra && producto is not null)
                    {
                        var talles = SepararOpciones(producto.Talles);
                        var colores = SepararOpciones(string.IsNullOrWhiteSpace(producto.Colores) ? producto.Color : producto.Colores);
                        if (talles.Length > 0 && talleNormalizado.Equals("Único", StringComparison.OrdinalIgnoreCase))
                        {
                            TempData["ErrorCarrito"] = "Debés elegir un talle para comprar el producto.";
                            return RedirectToAction("Index", "Home");
                        }
                        if (colores.Length > 1 && colorNormalizado.Equals("Único", StringComparison.OrdinalIgnoreCase))
                        {
                            TempData["ErrorCarrito"] = "Debés elegir un color para comprar el producto.";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    // El carrito puede seguir funcionando con productos del catálogo local si la API no está disponible.
                }
            }

            var carrito = await ObtenerCarritoAsync();

            // El ID identifica el producto aunque existan productos con el mismo nombre.
            var itemExistente = carrito.FirstOrDefault(p => 
                (productoId > 0 && p.ProductoId == productoId ||
                 productoId <= 0 && p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)) &&
                p.Talle.Equals(talleNormalizado, StringComparison.OrdinalIgnoreCase) &&
                p.Color.Equals(colorNormalizado, StringComparison.OrdinalIgnoreCase));

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
            }
            else
            {
                carrito.Add(new ItemCarrito
                {
                    ProductoId = productoId,
                    Nombre = nombre,
                    Precio = precio,
                    Imagen = imagen,
                    Talle = talleNormalizado,
                    Color = colorNormalizado,
                    Cantidad = cantidad
                });
            }

            await GuardarCarritoAsync(carrito);
            return RedirectToAction("Index");
        }

        // Sumar cantidad de un ítem específico en el carrito
        [HttpGet]
        public async Task<IActionResult> SumarCantidad(string nombre, string talle, string color)
        {
            var carrito = await ObtenerCarritoAsync();
            var talleNormalizado = string.IsNullOrWhiteSpace(talle) ? "Único" : talle;
            var colorNormalizado = string.IsNullOrWhiteSpace(color) ? "Único" : color;
            var item = carrito.FirstOrDefault(p => 
                p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) && 
                p.Talle.Equals(talleNormalizado, StringComparison.OrdinalIgnoreCase) &&
                p.Color.Equals(colorNormalizado, StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                item.Cantidad++;
                await GuardarCarritoAsync(carrito);
            }
            return RedirectToAction("Index");
        }

        // Restar cantidad de un ítem (si llega a 0 o menos, lo elimina)
        [HttpGet]
        public async Task<IActionResult> RestarCantidad(string nombre, string talle, string color)
        {
            var carrito = await ObtenerCarritoAsync();
            var talleNormalizado = string.IsNullOrWhiteSpace(talle) ? "Único" : talle;
            var colorNormalizado = string.IsNullOrWhiteSpace(color) ? "Único" : color;
            var item = carrito.FirstOrDefault(p => 
                p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) && 
                p.Talle.Equals(talleNormalizado, StringComparison.OrdinalIgnoreCase) &&
                p.Color.Equals(colorNormalizado, StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                item.Cantidad--;
                if (item.Cantidad <= 0)
                {
                    carrito.Remove(item);
                }
                await GuardarCarritoAsync(carrito);
            }
            return RedirectToAction("Index");
        }

        // Eliminar por completo un ítem del carrito
        [HttpGet]
        public async Task<IActionResult> EliminarItem(string nombre, string talle, string color)
        {
            var carrito = await ObtenerCarritoAsync();
            var talleNormalizado = string.IsNullOrWhiteSpace(talle) ? "Único" : talle;
            var colorNormalizado = string.IsNullOrWhiteSpace(color) ? "Único" : color;
            var item = carrito.FirstOrDefault(p => 
                p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) && 
                p.Talle.Equals(talleNormalizado, StringComparison.OrdinalIgnoreCase) &&
                p.Color.Equals(colorNormalizado, StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                carrito.Remove(item);
                await GuardarCarritoAsync(carrito);
            }
            return RedirectToAction("Index");
        }

        private static string[] SepararOpciones(string? valor)
        {
            return (valor ?? string.Empty)
                .Split(new[] { ',', ';', '/', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

       // Botón "FINALIZAR COMPRA"
        public async Task<IActionResult> FinalizarCompra()
        {
            // Validamos que el usuario esté logueado mediante la sesión de la tienda
            var usuarioLogueado = HttpContext.Session.GetString("UsuarioSesion");
            if (string.IsNullOrEmpty(usuarioLogueado))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Carrito") });
            }

            var carrito = await ObtenerCarritoAsync();
            if (!carrito.Any())
            {
                return RedirectToAction("Index");
            }

            // Redirige a la vista Envio.cshtml dentro de la carpeta Checkout
            return RedirectToAction("Envio", "Checkout");
        }
        // Métodos auxiliares privados para leer/escribir en Session usando JSON
        private async Task<List<ItemCarrito>> ObtenerCarritoAsync()
        {
            var sessionData = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "CarritoSession"));
            if (!string.IsNullOrEmpty(sessionData) &&
                !int.TryParse(HttpContext.Session.GetString("UsuarioApiId"), out _))
            {
                return JsonSerializer.Deserialize<List<ItemCarrito>>(sessionData) ?? new List<ItemCarrito>();
            }

            if (!int.TryParse(HttpContext.Session.GetString("UsuarioApiId"), out var usuarioId) || usuarioId <= 0)
            {
                if (!string.IsNullOrEmpty(sessionData))
                    return JsonSerializer.Deserialize<List<ItemCarrito>>(sessionData) ?? new List<ItemCarrito>();
                return new List<ItemCarrito>();
            }

            try
            {
                var carrito = await _httpClient.GetFromJsonAsync<List<ItemCarrito>>($"api/usuarios/{usuarioId}/carrito") ?? new List<ItemCarrito>();
                HttpContext.Session.SetString(UserSessionKeys.ForUser(HttpContext.Session, "CarritoSession"), JsonSerializer.Serialize(carrito));
                return carrito;
            }
            catch (HttpRequestException)
            {
                return string.IsNullOrEmpty(sessionData)
                    ? new List<ItemCarrito>()
                    : JsonSerializer.Deserialize<List<ItemCarrito>>(sessionData) ?? new List<ItemCarrito>();
            }
        }

        private async Task GuardarCarritoAsync(List<ItemCarrito> carrito)
        {
            var contenido = JsonSerializer.Serialize(carrito);
            HttpContext.Session.SetString(UserSessionKeys.ForUser(HttpContext.Session, "CarritoSession"), contenido);

            if (!int.TryParse(HttpContext.Session.GetString("UsuarioApiId"), out var usuarioId) || usuarioId <= 0)
                return;

            try
            {
                await _httpClient.PutAsJsonAsync($"api/usuarios/{usuarioId}/carrito", new { Contenido = contenido });
            }
            catch (HttpRequestException)
            {
                // La sesión conserva el carrito hasta que la API vuelva a estar disponible.
            }
        }
    }
}