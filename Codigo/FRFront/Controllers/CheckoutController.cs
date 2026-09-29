using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using FRFront.Helpers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FRFront.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly HttpClient _httpClient;

        public CheckoutController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
        }

        // 1. Muestra la pantalla de Envío
        public IActionResult Envio()
        {
            var sessionData = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "CarritoSession"));
            if (string.IsNullOrEmpty(sessionData))
            {
                return RedirectToAction("Index", "Carrito");
            }

            var carrito = JsonSerializer.Deserialize<List<ItemCarrito>>(sessionData) ?? new List<ItemCarrito>();
            return View(carrito);
        }

        // 2. Procesa el envío, calcula el costo y redirige al Paso 2: Pago
        [HttpPost]
        public IActionResult ProcesarEnvio(string tipoEnvio, string codigoPostal, string ciudad, string provincia, string domicilio, string numero, string destinatario, string? comentarios)
        {
            decimal costoEnvio = 0;
            string nombreEnvioTexto = "Retiro en el local";

            if (tipoEnvio == "Premium") // 1 día hábil (Sunchales - 2322)
            {
                costoEnvio = 3000;
                nombreEnvioTexto = "Envío a domicilio premium (1 día hábil)";
            }
            else if (tipoEnvio == "Estandar") // 7 días hábiles (Nacional)
            {
                costoEnvio = 15000;
                nombreEnvioTexto = "Envío a domicilio estándar (7 días hábiles)";
            }
            else if (tipoEnvio == "PremiumNacional") // 2 días hábiles (Nacional)
            {
                costoEnvio = 22000;
                nombreEnvioTexto = "Envío a domicilio premium (2 días hábiles)";
            }
            else if (tipoEnvio == "RetiroLocal")
            {
                costoEnvio = 0;
                nombreEnvioTexto = "Retiro en el local (Sunchales, Santa Fe)";
            }

            // Guardamos los datos del envío en la sesión
            HttpContext.Session.SetString(UserSessionKeys.ForUser(HttpContext.Session, "TipoEnvioSeleccionado"), tipoEnvio ?? "Estándar");
            HttpContext.Session.SetString(UserSessionKeys.ForUser(HttpContext.Session, "CodigoPostalEnvio"), codigoPostal ?? "");
            HttpContext.Session.SetString(UserSessionKeys.ForUser(HttpContext.Session, "NombreEnvio"), nombreEnvioTexto);
            HttpContext.Session.SetString(UserSessionKeys.ForUser(HttpContext.Session, "CostoEnvio"), costoEnvio.ToString());

            // Redirige a la vista Pago.cshtml dentro de la carpeta Checkout
            return RedirectToAction("Pago");
        }

        // 3. Muestra la pantalla de Pago
        public IActionResult Pago()
        {
            var sessionData = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "CarritoSession"));
            if (string.IsNullOrEmpty(sessionData))
            {
                return RedirectToAction("Index", "Carrito");
            }

            var carrito = JsonSerializer.Deserialize<List<ItemCarrito>>(sessionData) ?? new List<ItemCarrito>();

            // Recuperamos los datos del envío para mostrarlos en el resumen de pago
            ViewData["NombreEnvio"] = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "NombreEnvio")) ?? "Retiro en el local";
            
            decimal costoEnvio = 0;
            string? costoEnvioStr = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "CostoEnvio"));
            if (!string.IsNullOrEmpty(costoEnvioStr))
            {
                decimal.TryParse(costoEnvioStr, out costoEnvio);
            }
            ViewData["CostoEnvio"] = costoEnvio;

            return View(carrito);
        }

        // 4. Procesa el pago y finaliza la compra con éxito
        [HttpPost]
        public async Task<IActionResult> ProcesarPago(string metodoPago, string? numeroTarjeta, string? vencimiento, string? cvv, string? titular, int? cuotas)
        {
            var usuario = HttpContext.Session.GetString("UsuarioSesion");
            var sessionData = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "CarritoSession"));
            var carrito = string.IsNullOrEmpty(sessionData)
                ? new List<ItemCarrito>()
                : JsonSerializer.Deserialize<List<ItemCarrito>>(sessionData) ?? new List<ItemCarrito>();

            if (string.IsNullOrWhiteSpace(usuario) || carrito.Count == 0)
            {
                TempData["ErrorMessage"] = "No se pudo registrar la compra porque faltan datos del usuario o del carrito.";
                return RedirectToAction("Index", "Carrito");
            }

            try
            {
                var productosResponse = await _httpClient.GetFromJsonAsync<List<ProductoDto>>("api/productos") ?? new List<ProductoDto>();
                var productosPorNombre = productosResponse
                    .Where(producto => !string.IsNullOrWhiteSpace(producto.Nombre))
                    .ToDictionary(producto => producto.Nombre.Trim(), StringComparer.OrdinalIgnoreCase);

                var detalles = carrito.Select(item => new
                {
                    ProductoId = productosPorNombre.TryGetValue(item.Nombre.Trim(), out var producto) ? producto.Id : 0,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.Precio
                }).ToList();

                var costoEnvioTexto = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "CostoEnvio"));
                decimal.TryParse(costoEnvioTexto, out var costoEnvio);
                var total = carrito.Sum(item => item.Total) + costoEnvio;

                var pedidoRequest = new
                {
                    Cliente = usuario.Split('@')[0],
                    Email = usuario,
                    Total = total,
                    TipoEntrega = metodoPago,
                    Detalle = detalles
                };

                using var response = await _httpClient.PostAsJsonAsync("api/pedidos", pedidoRequest);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "No se pudo guardar la compra en la base de datos.";
                    return RedirectToAction("Pago");
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "No se pudo conectar con la API para guardar la compra.";
                return RedirectToAction("Pago");
            }

            // Redirige a la pantalla de éxito
            return RedirectToAction("CompraExitosas");
        }

        // 5. Pantallas de éxito o rechazo
         public IActionResult CompraExitosas()
        {
            var sessionData = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "CarritoSession"));
            var carrito = string.IsNullOrEmpty(sessionData) 
                ? new List<ItemCarrito>() 
                : JsonSerializer.Deserialize<List<ItemCarrito>>(sessionData) ?? new List<ItemCarrito>();

            string tipoEnvio = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "TipoEnvioSeleccionado")) ?? "Estandar";
            
            // Cálculo de fecha de entrega
            DateTime hoy = DateTime.Now;
            string detalleFecha = "";

            if (tipoEnvio == "RetiroLocal")
            {
                detalleFecha = "Disponible para retirar en el local";
            }
            else if (tipoEnvio == "Premium")
            {
                detalleFecha = $"Llega el {hoy.AddDays(1):dd} de {hoy.AddDays(1):MMMM}";
            }
            else
            {
                int dias = tipoEnvio == "PremiumNacional" ? 2 : 7;
                detalleFecha = $"Llega entre el {hoy.AddDays(2):dd} y el {hoy.AddDays(dias):dd} de {hoy:MMMM}";
            }

            Random rnd = new Random();
            string nroPedido = "#" + rnd.Next(10000, 99999).ToString();

            // Creamos el nuevo pedido
            var nuevoPedido = new PedidoModel
            {
                NroPedido = nroPedido,
                Estado = "En Camino",
                DetalleFecha = detalleFecha,
                TotalProductos = carrito.Sum(x => x.Cantidad),
                ImagenProducto = carrito.FirstOrDefault()?.Imagen ?? "~/images/logo-fr.png",
                Productos = carrito
            };

            // Recuperamos el historial actual de la sesión
            var historialJson = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "HistorialComprasSession"));
            var listaHistorial = string.IsNullOrEmpty(historialJson)
                ? new List<PedidoModel>()
                : JsonSerializer.Deserialize<List<PedidoModel>>(historialJson) ?? new List<PedidoModel>();

            // Agregamos el nuevo pedido al principio de la lista
            listaHistorial.Insert(0, nuevoPedido);

            // Guardamos el historial actualizado en la sesión
            HttpContext.Session.SetString(UserSessionKeys.ForUser(HttpContext.Session, "HistorialComprasSession"), JsonSerializer.Serialize(listaHistorial));

            ViewData["TextoEntrega"] = detalleFecha;
            ViewData["NumeroPedido"] = nroPedido;
            ViewData["EsRetiroLocal"] = tipoEnvio == "RetiroLocal";

            // Vaciamos el carrito actual
            HttpContext.Session.Remove(UserSessionKeys.ForUser(HttpContext.Session, "CarritoSession"));

            return View(carrito);
        }

        public IActionResult PagoRechazado()
        {
            return View();
        }
    }
}