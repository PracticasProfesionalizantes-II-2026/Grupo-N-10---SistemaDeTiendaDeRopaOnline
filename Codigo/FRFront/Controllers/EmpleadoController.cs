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
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };
        }

        private async Task<Dictionary<int, string>> ObtenerDiccionarioUsuariosAsync()
        {
            var mapaUsuarios = new Dictionary<int, string>();
            try
            {
                var response = await _httpClient.GetAsync("api/usuarios");
                if (!response.IsSuccessStatusCode)
                {
                    response = await _httpClient.GetAsync("api/clientes");
                }

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var listaUsuarios = JsonSerializer.Deserialize<List<UsuarioSimpleDto>>(content, _jsonOptions);
                    
                    if (listaUsuarios != null)
                    {
                        foreach (var user in listaUsuarios)
                        {
                            string nombreCompleto = $"{user.Nombre} {user.Apellido}".Trim();
                            if (user.Id > 0 && !string.IsNullOrWhiteSpace(nombreCompleto))
                            {
                                mapaUsuarios[user.Id] = nombreCompleto;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR AL CONSULTAR USUARIOS]: {ex.Message}");
            }

            return mapaUsuarios;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Empleado/Index.cshtml");
        }

        // ==========================================
        // PUNTO DE VENTA (POS / COBRAR)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> PuntoVenta()
        {
            var productos = new List<ProductoDto>();
            try
            {
                var response = await _httpClient.GetAsync("api/productos");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiProds = JsonSerializer.Deserialize<List<ProductoDto>>(content, _jsonOptions);
                    if (apiProds != null)
                    {
                        productos = apiProds;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EN PUNTO DE VENTA]: {ex.Message}");
            }

            return View("~/Views/Empleado/PuntoVenta.cshtml", productos);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarVenta([FromBody] PedidoVentaRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, message = "Datos de la venta no válidos" });
                }

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/pedidos", content);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Venta registrada dinámicamente con éxito" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

            return Json(new { success = false, message = "Error al procesar la venta en la API" });
        }

        // ==========================================
        // PEDIDOS DEL CAJERO
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Pedidos()
        {
            var listaPedidos = new List<PedidoDto>();

            try
            {
                var mapaUsuarios = await ObtenerDiccionarioUsuariosAsync();
                var response = await _httpClient.GetAsync("api/pedidos");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(content))
                    {
                        foreach (var item in doc.RootElement.EnumerateArray())
                        {
                            int idPedido = item.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0;
                            int usuarioId = item.TryGetProperty("usuarioId", out var uProp) ? uProp.GetInt32() : 0;
                            decimal totalPedido = item.TryGetProperty("total", out var totalProp) ? totalProp.GetDecimal() : 0;
                            string metodoPago = item.TryGetProperty("metodoPago", out var pagoProp) ? pagoProp.GetString() ?? "EFECTIVO" : "EFECTIVO";
                            DateTime fechaPedido = item.TryGetProperty("fechaPedido", out var fechaProp) ? fechaProp.GetDateTime() : DateTime.Now;

                            string nombreCliente = mapaUsuarios.TryGetValue(usuarioId, out var nombre) && !string.IsNullOrWhiteSpace(nombre) 
                                ? nombre 
                                : $"Cliente #{usuarioId}";

                            listaPedidos.Add(new PedidoDto
                            {
                                Id = idPedido,
                                Cliente = nombreCliente,
                                Fecha = fechaPedido,
                                Total = totalPedido,
                                Estado = "CONFIRMADO",
                                TipoEntrega = metodoPago
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EN PEDIDOS CAJERO]: {ex.Message}");
            }

            listaPedidos = listaPedidos.OrderByDescending(p => p.Fecha).ThenByDescending(p => p.Id).ToList();

            return View("~/Views/Empleado/Pedidos.cshtml", listaPedidos);
        }

        // ==========================================
        // FACTURAS DEL CAJERO
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Facturas()
        {
            var listaFacturas = new List<PedidoDto>();

            try
            {
                var mapaUsuarios = await ObtenerDiccionarioUsuariosAsync();

                var mapaPedidosUsuario = new Dictionary<int, int>();
                var responsePedidos = await _httpClient.GetAsync("api/pedidos");
                if (responsePedidos.IsSuccessStatusCode)
                {
                    var contentP = await responsePedidos.Content.ReadAsStringAsync();
                    using (var docP = JsonDocument.Parse(contentP))
                    {
                        foreach (var itemP in docP.RootElement.EnumerateArray())
                        {
                            int pId = itemP.TryGetProperty("id", out var pProp) ? pProp.GetInt32() : 0;
                            int uId = itemP.TryGetProperty("usuarioId", out var uProp) ? uProp.GetInt32() : 0;
                            if (pId > 0) mapaPedidosUsuario[pId] = uId;
                        }
                    }
                }

                var responseFacturas = await _httpClient.GetAsync("api/facturas");
                if (responseFacturas.IsSuccessStatusCode)
                {
                    var contentF = await responseFacturas.Content.ReadAsStringAsync();
                    using (var docF = JsonDocument.Parse(contentF))
                    {
                        foreach (var elemento in docF.RootElement.EnumerateArray())
                        {
                            int idFactura = elemento.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0;
                            int pedidoId = elemento.TryGetProperty("pedidoId", out var pProp) ? pProp.GetInt32() : 0;
                            decimal totalFactura = elemento.TryGetProperty("total", out var totalProp) ? totalProp.GetDecimal() : 0;
                            DateTime fechaFactura = elemento.TryGetProperty("fecha", out var fechaProp) ? fechaProp.GetDateTime() : DateTime.Now;

                            int usuarioId = mapaPedidosUsuario.TryGetValue(pedidoId, out var uid) ? uid : 0;
                            string nombreCliente = mapaUsuarios.TryGetValue(usuarioId, out var nombre) && !string.IsNullOrWhiteSpace(nombre) 
                                ? nombre 
                                : $"Cliente #{usuarioId}";

                            listaFacturas.Add(new PedidoDto
                            {
                                Id = idFactura,
                                Cliente = nombreCliente,
                                Fecha = fechaFactura,
                                Total = totalFactura,
                                Estado = "PENDIENTE",
                                TipoEntrega = "EFECTIVO"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EN FACTURAS CAJERO]: {ex.Message}");
            }

            return View("~/Views/Empleado/Facturas.cshtml", listaFacturas.OrderByDescending(f => f.Fecha).ThenByDescending(f => f.Id).ToList());
        }

        // ==========================================
        // CLIENTES EN PANEL CAJERO
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Clientes(string? busqueda)
        {
            var clientes = new List<ClienteDto>();

            try
            {
                var response = await _httpClient.GetAsync("api/clientes");
                if (!response.IsSuccessStatusCode)
                {
                    response = await _httpClient.GetAsync("api/usuarios");
                }

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiClientes = JsonSerializer.Deserialize<List<ClienteDto>>(content, _jsonOptions);
                    if (apiClientes != null)
                    {
                        clientes = apiClientes;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EN CLIENTES CAJERO]: {ex.Message}");
            }

            if (!string.IsNullOrEmpty(busqueda))
            {
                clientes = clientes.Where(c => (c.Dni != null && c.Dni.Contains(busqueda, StringComparison.OrdinalIgnoreCase)) ||
                                               (c.NombreCompleto != null && c.NombreCompleto.Contains(busqueda, StringComparison.OrdinalIgnoreCase)) ||
                                               (c.Email != null && c.Email.Contains(busqueda, StringComparison.OrdinalIgnoreCase)) ||
                                               (c.Telefono != null && c.Telefono.Contains(busqueda, StringComparison.OrdinalIgnoreCase)) ||
                                               (c.NumeroCliente != null && c.NumeroCliente.Contains(busqueda, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            ViewBag.BusquedaActual = busqueda ?? "";

            return View("~/Views/Clientes/Index.cshtml", clientes);
        }
    }

    public class PedidoVentaRequest
    {
        public string Cliente { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string? TipoEntrega { get; set; }
        public List<DetallePedidoVentaRequest> Detalle { get; set; } = new();
    }

    public class DetallePedidoVentaRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}