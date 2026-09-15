using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using System.Text.Json;
using System.Text;

namespace FRFront.Controllers
{
    public class EmpleadoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public EmpleadoController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // GET: /Empleado/Index
        [HttpGet]
        public IActionResult Index()
        {
            string? rol = HttpContext.Session.GetString("RolSesion");

            if (string.IsNullOrEmpty(rol) || (rol != "Empleado" && rol != "Administrador"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View("~/Views/Empleado/Index.cshtml");
        }

        // GET: /Empleado/NuevaVenta
        [HttpGet]
        public IActionResult NuevaVenta()
        {
            string? rol = HttpContext.Session.GetString("RolSesion");

            if (string.IsNullOrEmpty(rol) || (rol != "Empleado" && rol != "Administrador"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View("~/Views/Empleado/NuevaVenta.cshtml");
        }

        // POST: /Empleado/ProcesarVenta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarVenta(
            string clienteDni, 
            string clienteNombre, 
            string clienteApellido, 
            string clienteEmail, 
            string clienteTelefono, 
            string metodoPago, 
            decimal totalVenta, 
            string jsonDetalle)
        {
            string? rol = HttpContext.Session.GetString("RolSesion");
            if (string.IsNullOrEmpty(rol))
            {
                return RedirectToAction("Login", "Account");
            }

            var detalles = string.IsNullOrEmpty(jsonDetalle) 
                ? new List<DetallePedidoDto>() 
                : JsonSerializer.Deserialize<List<DetallePedidoDto>>(jsonDetalle, _jsonOptions) ?? new List<DetallePedidoDto>();

            int nuevoId = 100 + Random.Shared.Next(100, 999);
            string nombreClienteCompleto = $"{clienteNombre.ToUpper()} {clienteApellido.ToUpper()}";

            var nuevoPedido = new PedidoDto
            {
                Id = nuevoId,
                Cliente = nombreClienteCompleto,
                Fecha = DateTime.Now,
                Total = totalVenta,
                Estado = "ENTREGADO",
                TipoEntrega = $"VENTA MOSTRADOR ({metodoPago.Replace("_", " ")})",
                Detalle = detalles
            };

            // REGISTRO EN EL HISTORIAL COMPARTIDO (ADMIN, CAJERO Y CLIENTE)
            AdministradorController.RegistrarNuevoPedido(nuevoPedido, clienteDni, clienteEmail, clienteTelefono);

            TempData["SuccessMessage"] = $"¡Venta registrada con éxito! Factura #{nuevoId} creada para {nombreClienteCompleto} y guardada en el Historial de Compras.";

            return RedirectToAction("Index");
        }
    }
}