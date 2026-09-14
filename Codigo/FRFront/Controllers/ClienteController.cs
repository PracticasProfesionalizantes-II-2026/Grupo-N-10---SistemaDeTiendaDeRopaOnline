using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using System.Text.Json;

namespace FRFront.Controllers
{
    public class ClienteController : Controller
    {
        // Vista de Mis Compras del cliente autenticado con datos reales
        public IActionResult MisCompras()
        {
            var usuarioLogueado = HttpContext.Session.GetString("UsuarioSesion");
            if (string.IsNullOrEmpty(usuarioLogueado))
            {
                return RedirectToAction("Login", "Account");
            }

            // Recuperamos el historial de compras de la sesión
            var historialJson = HttpContext.Session.GetString("HistorialComprasSession");
            var listaCompras = string.IsNullOrEmpty(historialJson) 
                ? new List<PedidoModel>() 
                : JsonSerializer.Deserialize<List<PedidoModel>>(historialJson) ?? new List<PedidoModel>();

            // Si no hay compras registradas todavía, precargamos las de ejemplo para el diseño inicial
            if (!listaCompras.Any())
            {
                listaCompras = new List<PedidoModel>
                {
                    new PedidoModel
                    {
                        NroPedido = "#70341",
                        Estado = "Entregado",
                        DetalleFecha = "Llegó el 4 de marzo",
                        TotalProductos = 1,
                        ImagenProducto = "~/images/buzovcv.png"
                    },
                    new PedidoModel
                    {
                        NroPedido = "#70344",
                        Estado = "En Camino",
                        DetalleFecha = $"Llega entre el {DateTime.Now.AddDays(2):dd} y el {DateTime.Now.AddDays(5):dd} de {DateTime.Now:MMMM}",
                        TotalProductos = 1,
                        ImagenProducto = "~/images/buzovcv.png"
                    }
                };
            }

            return View(listaCompras);
        }

        // Vista de Notificaciones del cliente
        public IActionResult Notificaciones()
        {
            // Creamos una lista de prueba directamente para asegurar que la vista cargue con datos
            var listaNotificaciones = new List<NotificacionModel>
            {
                new NotificacionModel 
                { 
                    Titulo = "¡Tu pedido está en camino!", 
                    Mensaje = "El pedido reciente sale hoy hacia tu domicilio.", 
                    Fecha = "Hace 2 horas", 
                    Tipo = "Envio" 
                },
                new NotificacionModel 
                { 
                    Titulo = "¡15% OFF en Buzos!", 
                    Mensaje = "Aprovechá el descuento exclusivo por tiempo limitado en toda la tienda.", 
                    Fecha = "Ayer", 
                    Tipo = "Oferta" 
                }
            };

            // Retornamos la vista enviándole el modelo explícitamente
            return View(listaNotificaciones);
        }
    }
}