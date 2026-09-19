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

        // ==========================================
        // REDIRECCIONES DE PEDIDOS PARA EVITAR 404
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Pedidos()
        {
            return RedirectToAction("Pedidos", "Administrador");
        }

        [HttpGet]
        public IActionResult DetallePedido(int id)
        {
            return RedirectToAction("DetallePedido", "Administrador", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstadoPedido(int id, string nuevoEstado)
        {
            return RedirectToAction("ActualizarEstadoPedido", "Administrador", new { id = id, nuevoEstado = nuevoEstado });
        }

        // ==========================================
        // BÚSQUEDA Y PROCESAMIENTO DE VENTAS
        // ==========================================

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

            // 1. Guardar en memoria local para respuesta rápida en interfaz
            AdministradorController.RegistrarNuevoPedido(nuevoPedido, dniLimpio, clienteEmail, telefonoLimpio);

            // 2. Enviar a la API (FYR_DB)
            try
            {
                var bodyApi = new
                {
                    Cliente = nombreClienteCompleto,
                    Email = string.IsNullOrEmpty(clienteEmail) ? $"{clienteNombre.Trim().ToLower()}@mail.com" : clienteEmail,
                    Total = totalVenta,
                    TipoEntrega = metodoPago,
                    Detalle = detalles.Select(d => new
                    {
                        ProductoId = d.ProductoId > 0 ? d.ProductoId : 7,
                        Cantidad = d.Cantidad > 0 ? d.Cantidad : 1,
                        PrecioUnitario = d.PrecioUnitario > 0 ? d.PrecioUnitario : totalVenta
                    }).ToList()
                };

                var jsonBody = JsonSerializer.Serialize(bodyApi);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/pedidos", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Atención: La venta figurará en pantalla, pero la API respondió: {response.StatusCode}";
                }
                else
                {
                    TempData["SuccessMessage"] = $"¡Venta registrada con éxito! Factura #{nuevoId} guardada en FYR_DB para {nombreClienteCompleto}.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Excepción de conexión con la API: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}