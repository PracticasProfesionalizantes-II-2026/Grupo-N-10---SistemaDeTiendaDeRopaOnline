using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using System.Text.Json;

namespace FRFront.Controllers
{
    public class CheckoutController : Controller
    {
        // PASO 1: Vista de Detalle de Envío
        public IActionResult Envio()
        {
            var usuarioLogueado = HttpContext.Session.GetString("UsuarioSesion");
            if (string.IsNullOrEmpty(usuarioLogueado))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Envio", "Checkout") });
            }

            var carrito = ObtenerCarritoSesion();
            if (!carrito.Any())
            {
                return RedirectToAction("Index", "Carrito");
            }

            return View(carrito);
        }

        // Procesar formulario de Envío, calcular precio y pasar al Paso 2 (Pago)
        [HttpPost]
        public IActionResult ProcesarEnvio(string tipoEnvio, string codigoPostal, string ciudad, string provincia, string calle, string numero, string destinatario, string comentarios)
        {
            decimal costoEnvio = 0;
            string nombreEnvio = "";

            if (codigoPostal == "2322")
            {
                if (tipoEnvio == "Premium")
                {
                    costoEnvio = 6000;
                    nombreEnvio = "Envío a domicilio premium - Sunchales (2 días hábiles)";
                }
                else
                {
                    costoEnvio = 0;
                    nombreEnvio = "Retiro en el local (Sunchales, Santa Fe)";
                }
            }
            else
            {
                if (tipoEnvio == "Normal")
                {
                    costoEnvio = 10000;
                    nombreEnvio = "Envío a domicilio nacional (7 días hábiles)";
                }
                else if (tipoEnvio == "Premium")
                {
                    costoEnvio = 20000;
                    nombreEnvio = "Envío a domicilio premium nacional (2 días hábiles)";
                }
                else
                {
                    costoEnvio = 0;
                    nombreEnvio = "Retiro en el local (Sunchales, Santa Fe)";
                }
            }

            // Guardamos los datos en la sesión
            HttpContext.Session.SetString("TipoEnvio", nombreEnvio);
            HttpContext.Session.SetString("Destinatario", destinatario ?? "");
            HttpContext.Session.SetString("DireccionEnvio", $"{calle} {numero}, {ciudad}, {provincia} (CP: {codigoPostal})");
            HttpContext.Session.SetString("CostoEnvio", costoEnvio.ToString());

            return RedirectToAction("Pago");
        }

        public IActionResult Pago()
        {
            var carrito = ObtenerCarritoSesion();
            if (!carrito.Any())
            {
                return RedirectToAction("Index", "Carrito");
            }

            return View(carrito);
        }

        // Procesar el pago exitoso al hacer clic en "Pagar"
        [HttpPost]
        public IActionResult ProcesarPago(string metodoPago, int? cuotas)
        {
            HttpContext.Session.SetString("MetodoPago", metodoPago ?? "Efectivo");

            var carrito = ObtenerCarritoSesion();
            if (carrito.Any())
            {
                // 1. Guardamos una copia exacta para la pantalla de éxito
                string carritoJson = JsonSerializer.Serialize(carrito);
                HttpContext.Session.SetString("UltimaCompra", carritoJson);

                // 2. Generamos un número de pedido único y aleatorio
                string nroPedido = "#" + new Random().Next(10000, 99999).ToString();
                HttpContext.Session.SetString("NroPedido", nroPedido);

                // 3. Registramos la compra en el Historial del Cliente para la pantalla "Mis Compras"
                var nuevoPedido = new PedidoModel
                {
                    NroPedido = nroPedido,
                    Estado = "En Camino",
                    DetalleFecha = $"Llega entre el {DateTime.Now.AddDays(2):dd} y el {DateTime.Now.AddDays(5):dd} de {DateTime.Now:MMMM}",
                    TotalProductos = carrito.Sum(i => i.Cantidad),
                    ImagenProducto = carrito.First().Imagen
                };

                var historialJson = HttpContext.Session.GetString("HistorialComprasSession");
                var listaHistorial = string.IsNullOrEmpty(historialJson) 
                    ? new List<PedidoModel>() 
                    : JsonSerializer.Deserialize<List<PedidoModel>>(historialJson) ?? new List<PedidoModel>();
                
                listaHistorial.Insert(0, nuevoPedido); // Agregamos la más reciente arriba
                HttpContext.Session.SetString("HistorialComprasSession", JsonSerializer.Serialize(listaHistorial));

                // 4. Vaciamos el carrito de compras principal
                HttpContext.Session.Remove("CarritoSession");
            }

            return RedirectToAction("CompraExitosas");
        }

        // Vista de pago exitoso
        public IActionResult CompraExitosas()
        {
            var nroPedido = HttpContext.Session.GetString("NroPedido") ?? "#48192";
            ViewData["NroPedido"] = nroPedido;

            var sessionData = HttpContext.Session.GetString("UltimaCompra");
            var ultimaCompra = string.IsNullOrEmpty(sessionData) 
                ? new List<ItemCarrito>() 
                : JsonSerializer.Deserialize<List<ItemCarrito>>(sessionData) ?? new List<ItemCarrito>();

            return View(ultimaCompra);
        }

        // Vista de Pago Rechazado
        public IActionResult PagoRechazado()
        {
            return View();
        }

        private List<ItemCarrito> ObtenerCarritoSesion()
        {
            var sessionData = HttpContext.Session.GetString("CarritoSession");
            if (string.IsNullOrEmpty(sessionData))
            {
                return new List<ItemCarrito>();
            }
            return JsonSerializer.Deserialize<List<ItemCarrito>>(sessionData) ?? new List<ItemCarrito>();
        }
    }
}