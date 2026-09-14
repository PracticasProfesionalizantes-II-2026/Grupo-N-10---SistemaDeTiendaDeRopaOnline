using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using System.Text.Json;

namespace FRFront.Controllers
{
    public class CarritoController : Controller
    {
        // Simulamos una lista estática de productos o tu repositorio principal
        // (Acá deberías buscar el producto en tu lista general por nombre o ID)
        
        public IActionResult Index()
        {
            var carrito = ObtenerCarritoSesion();
            return View(carrito);
        }

        // Agregar al carrito (Funciona sin estar logueado)
        [HttpPost]
        public IActionResult Agregar(string nombre, decimal precio, string imagen, int cantidad = 1)
        {
            var carrito = ObtenerCarritoSesion();

            var itemExistente = carrito.FirstOrDefault(p => p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
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
                    Cantidad = cantidad
                });
            }

            GuardarCarritoSesion(carrito);
            return RedirectToAction("Index");
        }

        // Botón "FINALIZAR COMPRA"
        public IActionResult FinalizarCompra()
        {
            // REGLA: Si NO está logueado, lo mandamos al Login
            if (User.Identity?.IsAuthenticated != true)
            {
                // Guardamos la intención de compra o lo redirigimos al login indicando la ruta
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Carrito") });
            }

            // Si está logueado, lo dejamos pasar a la pasarela de pago o confirmación
            var carrito = ObtenerCarritoSesion();
            if (!carrito.Any())
            {
                return RedirectToAction("Index");
            }

            return View("Checkout", carrito);
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