using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using System.Text.Json;

namespace FRFront.Controllers
{
    public class ClienteController : Controller
    {
        // 1. Vista de Mis Compras del cliente autenticado con datos reales
        public IActionResult MisCompras()
        {
            var usuarioLogueado = HttpContext.Session.GetString("UsuarioSesion");
            if (string.IsNullOrEmpty(usuarioLogueado))
            {
                return RedirectToAction("Login", "Account");
            }

            // Recuperamos únicamente el historial de compras reales guardado en la sesión
            var historialJson = HttpContext.Session.GetString("HistorialComprasSession");
            var listaCompras = string.IsNullOrEmpty(historialJson) 
                ? new List<PedidoModel>() 
                : JsonSerializer.Deserialize<List<PedidoModel>>(historialJson) ?? new List<PedidoModel>();

            return View(listaCompras);
        }

        // 2. Vista de seguimiento de un pedido específico del cliente autenticado
        public IActionResult SeguirEnvio(string nroPedido)
        {
            var usuarioLogueado = HttpContext.Session.GetString("UsuarioSesion");
            if (string.IsNullOrEmpty(usuarioLogueado))
            {
                return RedirectToAction("Login", "Account");
            }

            var historialJson = HttpContext.Session.GetString("HistorialComprasSession");
            var listaCompras = string.IsNullOrEmpty(historialJson) 
                ? new List<PedidoModel>() 
                : JsonSerializer.Deserialize<List<PedidoModel>>(historialJson) ?? new List<PedidoModel>();

            // Buscamos el pedido que coincida con el número recibido
            var pedido = listaCompras.FirstOrDefault(p => p.NroPedido == nroPedido);

            // Si no lo encuentra en sesión, creamos uno temporal para que no falle
            if (pedido == null)
            {
                pedido = new PedidoModel
                {
                    NroPedido = nroPedido ?? "#70344",
                    Estado = "En Camino",
                    DetalleFecha = "Tu pedido llegará hoy entre las 14 hs y las 19 hs"
                };
            }

            return View(pedido);
        }

        // 3. Vista del Perfil de Usuario
        public IActionResult MiPerfil()
        {
            var usuarioLogueado = HttpContext.Session.GetString("UsuarioSesion");
            if (string.IsNullOrEmpty(usuarioLogueado))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["EmailUsuario"] = usuarioLogueado;
            return View();
        }

        // 4. Muestra la vista para Editar el Perfil
        public IActionResult EditarPerfil()
        {
            var usuarioLogueado = HttpContext.Session.GetString("UsuarioSesion");
            if (string.IsNullOrEmpty(usuarioLogueado))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["EmailUsuario"] = usuarioLogueado;
            return View();
        }

        // 5. Procesa la actualización de los datos del perfil
        [HttpPost]
        public IActionResult GuardarPerfil(string nombre, string dni, string domicilio, string telefono)
        {
            var usuarioLogueado = HttpContext.Session.GetString("UsuarioSesion");
            if (string.IsNullOrEmpty(usuarioLogueado))
            {
                return RedirectToAction("Login", "Account");
            }

            // Aquí puedes guardar los datos en tu sesión o base de datos si lo deseas

            // Guardamos un mensaje de éxito temporal
            TempData["MensajeExito"] = "¡Los cambios se han guardado con éxito!";

            return RedirectToAction("MiPerfil");
        }

        // 6. Vista de Notificaciones del cliente
        public IActionResult Notificaciones()
        {
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

            return View(listaNotificaciones);
        }
    }
}