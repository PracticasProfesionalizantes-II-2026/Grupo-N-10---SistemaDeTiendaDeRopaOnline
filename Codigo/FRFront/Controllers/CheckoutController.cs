using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using System.Text.Json;

namespace FRFront.Controllers
{
    public class CheckoutController : Controller
    {
        // 1. Muestra la pantalla de Envío
        public IActionResult Envio()
        {
            var sessionData = HttpContext.Session.GetString("CarritoSession");
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
            HttpContext.Session.SetString("TipoEnvioSeleccionado", tipoEnvio ?? "Estándar");
            HttpContext.Session.SetString("CodigoPostalEnvio", codigoPostal ?? "");
            HttpContext.Session.SetString("NombreEnvio", nombreEnvioTexto);
            HttpContext.Session.SetString("CostoEnvio", costoEnvio.ToString());

            // Redirige a la vista Pago.cshtml dentro de la carpeta Checkout
            return RedirectToAction("Pago");
        }

        // 3. Muestra la pantalla de Pago
        public IActionResult Pago()
        {
            var sessionData = HttpContext.Session.GetString("CarritoSession");
            if (string.IsNullOrEmpty(sessionData))
            {
                return RedirectToAction("Index", "Carrito");
            }

            var carrito = JsonSerializer.Deserialize<List<ItemCarrito>>(sessionData) ?? new List<ItemCarrito>();

            // Recuperamos los datos del envío para mostrarlos en el resumen de pago
            ViewData["NombreEnvio"] = HttpContext.Session.GetString("NombreEnvio") ?? "Retiro en el local";
            
            decimal costoEnvio = 0;
            string? costoEnvioStr = HttpContext.Session.GetString("CostoEnvio");
            if (!string.IsNullOrEmpty(costoEnvioStr))
            {
                decimal.TryParse(costoEnvioStr, out costoEnvio);
            }
            ViewData["CostoEnvio"] = costoEnvio;

            return View(carrito);
        }

        // 4. Procesa el pago y finaliza la compra con éxito
        [HttpPost]
        public IActionResult ProcesarPago(string metodoPago, string? numeroTarjeta, string? vencimiento, string? cvv, string? titular, int? cuotas)
        {
            // Aquí puedes registrar el pedido o la orden si lo requieres más adelante

            // Redirige a la pantalla de éxito
            return RedirectToAction("CompraExitosas");
        }

        // 5. Pantallas de éxito o rechazo
         public IActionResult CompraExitosas()
        {
            var sessionData = HttpContext.Session.GetString("CarritoSession");
            var carrito = string.IsNullOrEmpty(sessionData) 
                ? new List<ItemCarrito>() 
                : JsonSerializer.Deserialize<List<ItemCarrito>>(sessionData) ?? new List<ItemCarrito>();

            string tipoEnvio = HttpContext.Session.GetString("TipoEnvioSeleccionado") ?? "Estandar";
            
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
            var historialJson = HttpContext.Session.GetString("HistorialComprasSession");
            var listaHistorial = string.IsNullOrEmpty(historialJson)
                ? new List<PedidoModel>()
                : JsonSerializer.Deserialize<List<PedidoModel>>(historialJson) ?? new List<PedidoModel>();

            // Agregamos el nuevo pedido al principio de la lista
            listaHistorial.Insert(0, nuevoPedido);

            // Guardamos el historial actualizado en la sesión
            HttpContext.Session.SetString("HistorialComprasSession", JsonSerializer.Serialize(listaHistorial));

            ViewData["TextoEntrega"] = detalleFecha;
            ViewData["NumeroPedido"] = nroPedido;
            ViewData["EsRetiroLocal"] = tipoEnvio == "RetiroLocal";

            // Vaciamos el carrito actual
            HttpContext.Session.Remove("CarritoSession");

            return View(carrito);
        }

        public IActionResult PagoRechazado()
        {
            return View();
        }
    }
}