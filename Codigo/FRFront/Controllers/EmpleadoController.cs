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

        // GET: /Empleado/BuscarClientePorDni?dni=...
        [HttpGet]
        public async Task<IActionResult> BuscarClientePorDni(string dni)
        {
            if (string.IsNullOrEmpty(dni))
            {
                return Json(new { existe = false });
            }

            try
            {
                var response = await _httpClient.GetAsync("api/clientes");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var clientes = JsonSerializer.Deserialize<List<ClienteDto>>(content, _jsonOptions);
                    
                    var clienteApi = clientes?.FirstOrDefault(c => c.Dni != null && c.Dni.Trim() == dni.Trim());
                    if (clienteApi != null)
                    {
                        return Json(new { 
                            existe = true, 
                            nombre = clienteApi.Nombre, 
                            apellido = clienteApi.Apellido, 
                            email = clienteApi.Email, 
                            telefono = string.IsNullOrEmpty(clienteApi.Telefono) ? "Sin registrar" : clienteApi.Telefono 
                        });
                    }
                }
            }
            catch { }

            var clienteMemoria = AdministradorController.ClientesEnMemoria
                .FirstOrDefault(c => c.Dni != null && c.Dni.Trim() == dni.Trim());

            if (clienteMemoria != null)
            {
                return Json(new { 
                    existe = true, 
                    nombre = clienteMemoria.Nombre, 
                    apellido = clienteMemoria.Apellido, 
                    email = clienteMemoria.Email, 
                    telefono = string.IsNullOrEmpty(clienteMemoria.Telefono) ? "Sin registrar" : clienteMemoria.Telefono 
                });
            }

            return Json(new { existe = false });
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
            string nombreClienteCompleto = $"{clienteNombre.Trim().ToUpper()} {clienteApellido.Trim().ToUpper()}";
            string dniLimpio = string.IsNullOrEmpty(clienteDni) ? "S/D" : clienteDni.Trim();
            string telefonoLimpio = string.IsNullOrEmpty(clienteTelefono) ? "Sin registrar" : clienteTelefono.Trim();

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

            // Registro garantizado en memoria local asignando el DNI y Teléfono real
            AdministradorController.RegistrarNuevoPedido(nuevoPedido, dniLimpio, clienteEmail, telefonoLimpio);

            // Persistencia en la API Backend (FYR_DB)
            try
            {
                var bodyApi = new
                {
                    Dni = dniLimpio,
                    Nombre = clienteNombre.Trim().ToUpper(),
                    Apellido = clienteApellido.Trim().ToUpper(),
                    ClienteNombre = nombreClienteCompleto,
                    Email = clienteEmail,
                    Telefono = telefonoLimpio,
                    Phone = telefonoLimpio,
                    Total = totalVenta,
                    MetodoPago = metodoPago,
                    Estado = "ENTREGADO",
                    Detalles = detalles
                };

                var jsonBody = JsonSerializer.Serialize(bodyApi);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                await _httpClient.PostAsync("api/pedidos", content);
            }
            catch { }

            TempData["SuccessMessage"] = $"¡Venta registrada con éxito! Factura #{nuevoId} creada para {nombreClienteCompleto}.";

            return RedirectToAction("Index");
        }
    }
}