using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using System.Text.Json;

namespace FRFront.Controllers
{
    public class CarritoController : Controller
    {
        public IActionResult Index()
        {
            var carrito = ObtenerCarritoSesion();
            return View(carrito);
        }

        // Agregar al carrito (Funciona sin estar logueado, ahora incluye el talle)
        [HttpPost]
        public IActionResult Agregar(string nombre, decimal precio, string imagen, string talle, int cantidad = 1)
        {
            var carrito = ObtenerCarritoSesion();

            // Buscamos si ya existe el producto con el MISMO NOMBRE y el MISMO TALLE
            var itemExistente = carrito.FirstOrDefault(p => 
                p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) && 
                p.Talle.Equals(talle, StringComparison.OrdinalIgnoreCase));

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
            }
            else
            {
                carrito.Add(new ItemCarrito
                {
                    Nombre = nombre,
                    Precio = precio,
                    Imagen = imagen,
                    Talle = talle ?? "Único",
                    Cantidad = cantidad
                });
            }

            GuardarCarritoSesion(carrito);
            return RedirectToAction("Index");
        }

        // Sumar cantidad de un ítem específico en el carrito
        [HttpGet]
        public IActionResult SumarCantidad(string nombre, string talle)
        {
            var carrito = ObtenerCarritoSesion();
            var item = carrito.FirstOrDefault(p => 
                p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) && 
                p.Talle.Equals(talle, StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                item.Cantidad++;
                GuardarCarritoSesion(carrito);
            }
            return RedirectToAction("Index");
        }

        // Restar cantidad de un ítem (si llega a 0 o menos, lo elimina)
        [HttpGet]
        public IActionResult RestarCantidad(string nombre, string talle)
        {
            var carrito = ObtenerCarritoSesion();
            var item = carrito.FirstOrDefault(p => 
                p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) && 
                p.Talle.Equals(talle, StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                item.Cantidad--;
                if (item.Cantidad <= 0)
                {
                    carrito.Remove(item);
                }
                GuardarCarritoSesion(carrito);
            }
            return RedirectToAction("Index");
        }

        // Eliminar por completo un ítem del carrito
        [HttpGet]
        public IActionResult EliminarItem(string nombre, string talle)
        {
            var carrito = ObtenerCarritoSesion();
            var item = carrito.FirstOrDefault(p => 
                p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) && 
                p.Talle.Equals(talle, StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                carrito.Remove(item);
                GuardarCarritoSesion(carrito);
            }
            return RedirectToAction("Index");
        }

       // Botón "FINALIZAR COMPRA"
        public IActionResult FinalizarCompra()
        {
            // Validamos que el usuario esté logueado mediante la sesión de la tienda
            var usuarioLogueado = HttpContext.Session.GetString("UsuarioSesion");
            if (string.IsNullOrEmpty(usuarioLogueado))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Carrito") });
            }

            var carrito = ObtenerCarritoSesion();
            if (!carrito.Any())
            {
                return RedirectToAction("Index");
            }

            // Redirige a la vista Envio.cshtml dentro de la carpeta Checkout
            return RedirectToAction("Envio", "Checkout");
        }
        // Métodos auxiliares privados para leer/escribir en Session usando JSON
        private List<ItemCarrito> ObtenerCarritoSesion()
        {
            var sessionData = HttpContext.Session.GetString("CarritoSession");
            if (string.IsNullOrEmpty(sessionData))
            {
                return new List<ItemCarrito>();
            }
            return JsonSerializer.Deserialize<List<ItemCarrito>>(sessionData) ?? new List<ItemCarrito>();
        }

        private void GuardarCarritoSesion(List<ItemCarrito> carrito)
        {
            HttpContext.Session.SetString("CarritoSession", JsonSerializer.Serialize(carrito));
        }
    }
}