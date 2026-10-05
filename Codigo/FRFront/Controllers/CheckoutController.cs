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

            if (tipoEnvio == "Estandar")
            {
                costoEnvio = 6000;
                nombreEnvioTexto = "Envío normal (máximo 7 días)";
            }
            else if (tipoEnvio == "PremiumNacional")
            {
                costoEnvio = 8500;
                nombreEnvioTexto = "Envío full (máximo 2 días)";
            }
            else if (tipoEnvio == "RetiroLocal")
            {
                costoEnvio = 0;
                nombreEnvioTexto = "Retiro en el local (Sunchales, Santa Fe)";
            }

            // Guardamos los datos del envío en la sesión
            HttpContext.Session.SetString(UserSessionKeys.ForUser(HttpContext.Session, "TipoEnvioSeleccionado"), tipoEnvio ?? "Estandar");
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

            if (string.IsNullOrWhiteSpace(metodoPago))
            {
                TempData["ErrorMessage"] = "Seleccioná un método de pago para continuar.";
                return RedirectToAction("Pago");
            }

            if ((metodoPago.Equals("Debito", StringComparison.OrdinalIgnoreCase) || metodoPago.Equals("Credito", StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(numeroTarjeta) || string.IsNullOrWhiteSpace(vencimiento) || string.IsNullOrWhiteSpace(cvv) || string.IsNullOrWhiteSpace(titular)))
            {
                TempData["ErrorMessage"] = "Completá todos los datos de la tarjeta para continuar.";
                return RedirectToAction("Pago");
            }

            try
            {
                var usuarios = await _httpClient.GetFromJsonAsync<List<UsuarioSimpleDto>>("api/usuarios") ?? new List<UsuarioSimpleDto>();
                var usuarioApi = usuarios.FirstOrDefault(item => string.Equals(item.Email, usuario, StringComparison.OrdinalIgnoreCase));
                if (usuarioApi == null || usuarioApi.ResolveId() <= 0)
                {
                    TempData["ErrorMessage"] = "No se encontró el usuario de la sesión para registrar la compra.";
                    return RedirectToAction("Pago");
                }

                var productosResponse = await _httpClient.GetFromJsonAsync<List<ProductoDto>>("api/productos") ?? new List<ProductoDto>();
                var productosPorId = productosResponse
                    .Where(producto => producto.Id > 0)
                    .GroupBy(producto => producto.Id)
                    .ToDictionary(grupo => grupo.Key, grupo => grupo.First());
                var productosPorNombre = productosResponse
                    .Where(producto => !string.IsNullOrWhiteSpace(producto.Nombre))
                    .GroupBy(producto => producto.Nombre.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(grupo => grupo.Key, grupo => grupo.First(), StringComparer.OrdinalIgnoreCase);

                var detalles = carrito.Select(item => new
                {
                    ProductoId = item.ProductoId > 0 && productosPorId.TryGetValue(item.ProductoId, out var productoPorId)
                        ? productoPorId.Id
                        : productosPorNombre.TryGetValue(item.Nombre.Trim(), out var productoPorNombre)
                            ? productoPorNombre.Id
                            : 0,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.Precio
                }).ToList();

                var costoEnvioTexto = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "CostoEnvio"));
                decimal.TryParse(costoEnvioTexto, out var costoEnvio);
                var subtotal = carrito.Sum(item => item.Total);
                var ajustePago = metodoPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase)
                    ? subtotal * -0.20m
                    : metodoPago.Equals("Credito", StringComparison.OrdinalIgnoreCase) && cuotas == 6
                        ? subtotal * 0.20m
                        : metodoPago.Equals("Credito", StringComparison.OrdinalIgnoreCase) && cuotas == 12
                            ? subtotal * 0.40m
                            : 0m;
                var total = subtotal + ajustePago + costoEnvio;
                var tipoEnvio = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "TipoEnvioSeleccionado"));
                var esRetiroLocal = string.Equals(tipoEnvio, "RetiroLocal", StringComparison.OrdinalIgnoreCase);
                HttpContext.Session.SetString(UserSessionKeys.ForUser(HttpContext.Session, "UltimoTotalCompra"), total.ToString(System.Globalization.CultureInfo.InvariantCulture));

                var pedidoRequest = new
                {
                    DireccionEntrega = esRetiroLocal
                        ? "RETIRO_LOCAL"
                        : HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "CodigoPostalEnvio")) ?? "",
                    MetodoPago = metodoPago.Equals("Credito", StringComparison.OrdinalIgnoreCase)
                        ? $"Tarjeta de crédito - {cuotas.GetValueOrDefault(1)} cuota{(cuotas.GetValueOrDefault(1) == 1 ? string.Empty : "s")}"
                        : metodoPago.Equals("Debito", StringComparison.OrdinalIgnoreCase)
                            ? "Tarjeta de débito"
                            : metodoPago.Equals("Transferencia", StringComparison.OrdinalIgnoreCase)
                                ? "Transferencia"
                                : "Efectivo",
                    Total = total,
                    UsuarioId = usuarioApi.ResolveId(),
                    Estado = esRetiroLocal ? "Confirmado" : "EnCamino"
                };

                using var response = await _httpClient.PostAsJsonAsync("api/pedidos", pedidoRequest);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "No se pudo guardar la compra en la base de datos.";
                    return RedirectToAction("Pago");
                }

                var contenidoRespuesta = await response.Content.ReadAsStringAsync();
                var idPedido = ObtenerIdPedido(contenidoRespuesta);
                if (idPedido <= 0 && response.Headers.Location != null)
                {
                    var ultimoSegmento = response.Headers.Location.Segments.LastOrDefault()?.Trim('/');
                    int.TryParse(ultimoSegmento, out idPedido);
                }

                if (idPedido > 0)
                {
                    HttpContext.Session.SetString(UserSessionKeys.ForUser(HttpContext.Session, "PedidoPersistidoSesion"), idPedido.ToString());

                    foreach (var detalle in detalles.Where(detalle => detalle.ProductoId > 0))
                    {
                        try
                        {
                            await _httpClient.PostAsJsonAsync($"api/pedidos/{idPedido}/detalles", detalle);
                        }
                        catch (HttpRequestException)
                        {
                            // El pedido ya fue creado; el historial de sesión conserva los productos comprados.
                        }
                    }

                    // El carrito persistido también debe quedar vacío después de una compra confirmada.
                    try
                    {
                        await _httpClient.PutAsJsonAsync(
                            $"api/usuarios/{usuarioApi.ResolveId()}/carrito",
                            new { Contenido = "[]" });
                    }
                    catch (HttpRequestException)
                    {
                        // La compra ya fue guardada; se informa para que pueda reintentarse la sincronización.
                    }
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

        private static int ObtenerIdPedido(string contenidoRespuesta)
        {
            if (string.IsNullOrWhiteSpace(contenidoRespuesta))
                return 0;

            try
            {
                using var documento = JsonDocument.Parse(contenidoRespuesta);
                var raiz = documento.RootElement;
                if ((raiz.TryGetProperty("idPedido", out var idPedido) || raiz.TryGetProperty("IdPedido", out idPedido)) && idPedido.TryGetInt32(out var id))
                    return id;
                if ((raiz.TryGetProperty("id", out var idAlternativo) || raiz.TryGetProperty("Id", out idAlternativo)) && idAlternativo.TryGetInt32(out id))
                    return id;
            }
            catch (JsonException)
            {
                return 0;
            }

            return 0;
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

            var pedidoPersistido = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "PedidoPersistidoSesion"));
            string nroPedido = int.TryParse(pedidoPersistido, out var pedidoId)
                ? $"#{pedidoId:D5}"
                : "#" + new Random().Next(10000, 99999).ToString();

            // Creamos el nuevo pedido
            var nuevoPedido = new PedidoModel
            {
                NroPedido = nroPedido,
                Estado = "En Camino",
                DetalleFecha = detalleFecha,
                Total = decimal.TryParse(HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "UltimoTotalCompra")), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var totalCompra) ? totalCompra : carrito.Sum(x => x.Total),
                EsRetiroLocal = tipoEnvio == "RetiroLocal",
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