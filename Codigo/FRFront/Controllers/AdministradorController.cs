using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using System.Text.Json;
using System.Text;

namespace FRFront.Controllers
{
    public class AdministradorController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public static List<ProductoDto> ProductosEnMemoria { get; set; } = new List<ProductoDto>();
        public static List<PedidoDto> PedidosEnMemoria { get; set; } = new List<PedidoDto>();
        public static List<ClienteDto> ClientesEnMemoria { get; set; } = new List<ClienteDto>();
        public static List<EmpleadoDto> EmpleadosEnMemoria { get; set; } = new List<EmpleadoDto>();
        
        public static ConfiguracionTiendaDto ConfiguracionActual { get; set; } = new ConfiguracionTiendaDto();

        public AdministradorController(IHttpClientFactory httpClientFactory)
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
                var endpoints = new[] { "api/usuarios", "api/clientes" };

                foreach (var endpoint in endpoints)
                {
                    var response = await _httpClient.GetAsync(endpoint);
                    if (!response.IsSuccessStatusCode)
                    {
                        continue;
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        continue;
                    }

                    var listaUsuarios = JsonSerializer.Deserialize<List<UsuarioSimpleDto>>(content, _jsonOptions);
                    if (listaUsuarios == null)
                    {
                        continue;
                    }

                    foreach (var user in listaUsuarios)
                    {
                        var userId = user.ResolveId();
                        var nombreCompleto = $"{user.Nombre} {user.Apellido}".Trim();

                        if (userId > 0 && !string.IsNullOrWhiteSpace(nombreCompleto))
                        {
                            mapaUsuarios[userId] = nombreCompleto;
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

        private static int ObtenerIdPedido(JsonElement elemento, string propiedadPrincipal, params string[] aliases)
        {
            var nombres = new[] { propiedadPrincipal }.Concat(aliases).ToArray();

            foreach (var nombre in nombres)
            {
                if (elemento.TryGetProperty(nombre, out var valor) && valor.ValueKind != JsonValueKind.Null)
                {
                    var entero = valor.GetInt32();
                    return entero;
                }
            }

            return 0;
        }

        private static string ObtenerNombreCliente(Dictionary<int, string> mapaUsuarios, int usuarioId)
        {
            if (usuarioId > 0 && mapaUsuarios.TryGetValue(usuarioId, out var nombre) && !string.IsNullOrWhiteSpace(nombre))
            {
                return nombre;
            }

            return usuarioId > 0 ? $"Cliente #{usuarioId}" : "Cliente Mostrador";
        }

        private static List<ClienteDto> NormalizarClientes(IEnumerable<ClienteDto> clientes)
        {
            var lista = clientes.ToList();
            foreach (var cliente in lista)
            {
                if (cliente.Id <= 0 && cliente.IdUsuario > 0)
                {
                    cliente.Id = cliente.IdUsuario;
                }
            }

            return lista;
        }

        [HttpGet]
        public IActionResult Index()
        {
            string? rol = HttpContext.Session.GetString("RolSesion");

            if (rol == "Empleado" || rol == "CAJERO")
            {
                return RedirectToAction("Index", "Empleado");
            }

            if (string.IsNullOrEmpty(rol) || rol != "Administrador")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // ==========================================
        // CONFIGURACIÓN DE LA TIENDA
        // ==========================================

        [HttpGet("Administrador/ConfigurarTienda")]
        [HttpGet("Administrador/Configuracion")]
        [ActionName("ConfigurarTienda")]
        public IActionResult ConfigurarTienda()
        {
            string? rol = HttpContext.Session.GetString("RolSesion");
            if (rol != "Administrador")
            {
                return RedirectToAction("Login", "Account");
            }

            return View("~/Views/Administrador/Configuracion.cshtml", ConfiguracionActual);
        }

        [HttpPost]
        [ActionName("GuardarConfiguracion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConfiguracion(ConfiguracionTiendaDto nuevaConfig, IFormFile? logoFile)
        {
            if (ModelState.IsValid)
            {
                ConfiguracionActual = nuevaConfig;

                if (logoFile != null && logoFile.Length > 0)
                {
                    ConfiguracionActual.LogoUrl = "/images/logo-fr.png";
                }

                TempData["SuccessMessage"] = "La configuración de la tienda ha sido guardada con éxito.";
                return RedirectToAction(nameof(ConfigurarTienda));
            }

            return View("~/Views/Administrador/Configuracion.cshtml", nuevaConfig);
        }

        // ==========================================
        // REPORTES DE VENTAS
        // ==========================================

        [HttpGet("Administrador/Reportes")]
        [HttpGet("Administrador/ReportesVentas")]
        [ActionName("Reportes")]
        public async Task<IActionResult> ReportesVentas(DateTime? desde, DateTime? hasta)
        {
            string? rol = HttpContext.Session.GetString("RolSesion");
            if (rol != "Administrador")
            {
                return RedirectToAction("Index", "Empleado");
            }

            DateTime fechaInicio = desde ?? DateTime.Today.AddDays(-30);
            DateTime fechaFin = hasta ?? DateTime.Today.AddDays(1).AddSeconds(-1);

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
                            int idPedido = ObtenerIdPedido(item, "id");
                            int usuarioId = ObtenerIdPedido(item, "usuarioId", "UsuarioId");
                            decimal totalPedido = item.TryGetProperty("total", out var totalProp) ? totalProp.GetDecimal() : 0;
                            string metodoPago = item.TryGetProperty("metodoPago", out var pagoProp) ? pagoProp.GetString() ?? "EFECTIVO" : "EFECTIVO";
                            DateTime fechaPedido = item.TryGetProperty("fechaPedido", out var fechaProp) ? fechaProp.GetDateTime() : DateTime.Now;

                            string clienteNombre = ObtenerNombreCliente(mapaUsuarios, usuarioId);

                            listaPedidos.Add(new PedidoDto
                            {
                                Id = idPedido,
                                Cliente = clienteNombre,
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
                Console.WriteLine($"[ERROR EN REPORTES API]: {ex.Message}");
            }

            var ventasFiltradas = listaPedidos
                .Where(p => p.Fecha.Date >= fechaInicio.Date && p.Fecha.Date <= fechaFin.Date)
                .Select(p => new VentaDetalleReporte
                {
                    PedidoId = p.Id,
                    Cliente = p.Cliente,
                    Fecha = p.Fecha,
                    TipoEntrega = p.TipoEntrega,
                    Estado = p.Estado,
                    Total = p.Total
                })
                .OrderByDescending(v => v.Fecha)
                .ToList();

            var datosGrafico = ventasFiltradas
                .GroupBy(v => v.Fecha.ToString("dd/MM/yyyy"))
                .Select(g => new { Fecha = g.Key, Total = g.Sum(x => x.Total) })
                .ToList();

            var reporteVentas = new ReporteVentasDto
            {
                FechaDesde = fechaInicio,
                FechaHasta = fechaFin,
                TotalVentas = ventasFiltradas.Sum(v => v.Total),
                TotalPedidos = ventasFiltradas.Count,
                ListadoVentas = ventasFiltradas,
                FechasGrafico = datosGrafico.Select(g => g.Fecha).ToList(),
                TotalesGrafico = datosGrafico.Select(g => g.Total).ToList()
            };

            return View("~/Views/Administrador/ReportesVentas.cshtml", reporteVentas);
        }

        // ==========================================
        // GESTIÓN DE PRODUCTOS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Productos(string? categoria, string? busqueda)
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
                Console.WriteLine($"[ERROR EN PRODUCTOS API]: {ex.Message}");
            }

            if (!string.IsNullOrEmpty(categoria) && !categoria.Equals("Todos", StringComparison.OrdinalIgnoreCase))
            {
                productos = productos.Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(busqueda))
            {
                productos = productos.Where(p => p.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                                                 p.Talles.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                                                 p.Color.Contains(busqueda, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.CategoriaSeleccionada = categoria ?? "Todos";
            ViewBag.BusquedaActual = busqueda ?? "";

            return View("~/Views/Productos/Index.cshtml", productos);
        }

        [HttpGet]
        public IActionResult CrearProducto()
        {
            return View("~/Views/Productos/Crear.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearProducto(ProductoDto nuevoProducto, IFormFile? imagenFile)
        {
            if (ModelState.IsValid)
            {
                nuevoProducto.Disponible = true;
                if (string.IsNullOrEmpty(nuevoProducto.ImagenUrl))
                {
                    nuevoProducto.ImagenUrl = "/images/hombres.png";
                }

                try
                {
                    var json = JsonSerializer.Serialize(nuevoProducto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync("api/productos", content);
                }
                catch { }

                TempData["SuccessMessage"] = "Producto creado con éxito.";
                return RedirectToAction(nameof(Productos));
            }

            return View("~/Views/Productos/Crear.cshtml", nuevoProducto);
        }

        [HttpGet]
        public async Task<IActionResult> EditarProducto(int id)
        {
            var producto = ProductosEnMemoria.FirstOrDefault(p => p.Id == id);
            return View("~/Views/Productos/Modificar.cshtml", producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProducto(ProductoDto productoModificado, IFormFile? imagenFile)
        {
            return RedirectToAction(nameof(Productos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            return RedirectToAction(nameof(Productos));
        }

        // ==========================================
        // GESTIÓN DE PEDIDOS Y DETALLE
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
                                UsuarioId = usuarioId,
                                Cliente = nombreCliente,
                                Fecha = fechaPedido,
                                Total = totalPedido,
                                Estado = "PAGADO",
                                TipoEntrega = metodoPago
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EN PEDIDOS API]: {ex.Message}");
            }

            listaPedidos = listaPedidos.OrderByDescending(p => p.Fecha).ThenByDescending(p => p.Id).ToList();

            return View("~/Views/Pedidos/Index.cshtml", listaPedidos);
        }

        [HttpGet]
        public async Task<IActionResult> DetallePedido(int id)
        {
            PedidoDto? pedido = null;

            try
            {
                var mapaUsuarios = await ObtenerDiccionarioUsuariosAsync();
                var response = await _httpClient.GetAsync($"api/pedidos/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    response = await _httpClient.GetAsync("api/pedidos");
                }

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(content))
                    {
                        var elemento = doc.RootElement.ValueKind == JsonValueKind.Array 
                            ? doc.RootElement.EnumerateArray().FirstOrDefault(x => x.TryGetProperty("id", out var p) && p.GetInt32() == id)
                            : doc.RootElement;

                        if (elemento.ValueKind != JsonValueKind.Undefined)
                        {
                            int idItem = elemento.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : id;
                            int usuarioId = elemento.TryGetProperty("usuarioId", out var uProp) ? uProp.GetInt32() : 0;
                            decimal total = elemento.TryGetProperty("total", out var totalProp) ? totalProp.GetDecimal() : 0;
                            string metodo = elemento.TryGetProperty("metodoPago", out var pagoProp) ? pagoProp.GetString() ?? "EFECTIVO" : "EFECTIVO";
                            DateTime fecha = elemento.TryGetProperty("fechaPedido", out var fechaProp) ? fechaProp.GetDateTime() : DateTime.Now;

                            string nombreCliente = mapaUsuarios.TryGetValue(usuarioId, out var nombre) && !string.IsNullOrWhiteSpace(nombre) 
                                ? nombre 
                                : $"Cliente #{usuarioId}";

                            var estadoPedido = elemento.TryGetProperty("estado", out var estadoProp)
                                ? estadoProp.ToString()
                                : string.Empty;
                            if (string.IsNullOrWhiteSpace(estadoPedido) || metodo.Equals("EFECTIVO", StringComparison.OrdinalIgnoreCase))
                            {
                                estadoPedido = "PAGADO";
                            }

                            pedido = new PedidoDto
                            {
                                Id = idItem,
                                UsuarioId = usuarioId,
                                Cliente = nombreCliente,
                                Fecha = fecha,
                                Total = total,
                                Estado = estadoPedido,
                                TipoEntrega = metodo
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EN DETALLE PEDIDO API]: {ex.Message}");
            }

            if (pedido == null)
            {
                pedido = new PedidoDto { Id = id, Cliente = "Cliente Desconocido", Total = 0, Fecha = DateTime.Now, Estado = "CONFIRMADO", TipoEntrega = "EFECTIVO" };
            }

            return View("~/Views/Pedidos/Detalle.cshtml", pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstadoPedido(int id, string nuevoEstado)
        {
            try
            {
                var estadoRequest = new StringContent(
                    JsonSerializer.Serialize(new { Estado = nuevoEstado }),
                    Encoding.UTF8,
                    "application/json");
                await _httpClient.PatchAsync($"api/pedidos/{id}/estado", estadoRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR AL ACTUALIZAR ESTADO DEL PEDIDO]: {ex.Message}");
            }

            return RedirectToAction(nameof(Pedidos));
        }

        // ==========================================
        // GESTIÓN DE CLIENTES
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
                        clientes = NormalizarClientes(apiClientes);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EN CLIENTES API]: {ex.Message}");
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

        [HttpGet]
        public async Task<IActionResult> DetalleCliente(int id)
        {
            var listaClientes = new List<ClienteDto>();

            try
            {
                var responseLista = await _httpClient.GetAsync("api/clientes");
                if (!responseLista.IsSuccessStatusCode)
                {
                    responseLista = await _httpClient.GetAsync("api/usuarios");
                }

                if (responseLista.IsSuccessStatusCode)
                {
                    var content = await responseLista.Content.ReadAsStringAsync();
                    var apiClientes = JsonSerializer.Deserialize<List<ClienteDto>>(content, _jsonOptions);
                    if (apiClientes != null)
                    {
                        listaClientes = NormalizarClientes(apiClientes);
                    }
                }
            }
            catch { }

            var cliente = listaClientes.FirstOrDefault(c => c.Id == id);
            if (cliente == null)
            {
                TempData["ErrorMessage"] = "Cliente no encontrado.";
                return RedirectToAction(nameof(Clientes));
            }

            return View("~/Views/Clientes/Detalle.cshtml", cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCliente(ClienteDto clienteModificado)
        {
            return RedirectToAction(nameof(Clientes));
        }

        [HttpGet]
        public async Task<IActionResult> HistorialCliente(int id)
        {
            var listaClientes = new List<ClienteDto>();
            try
            {
                var responseClientes = await _httpClient.GetAsync("api/clientes");
                if (!responseClientes.IsSuccessStatusCode)
                {
                    responseClientes = await _httpClient.GetAsync("api/usuarios");
                }

                if (responseClientes.IsSuccessStatusCode)
                {
                    var content = await responseClientes.Content.ReadAsStringAsync();
                    var apiClientes = JsonSerializer.Deserialize<List<ClienteDto>>(content, _jsonOptions);
                    if (apiClientes != null)
                    {
                        listaClientes = NormalizarClientes(apiClientes);
                    }
                }
            }
            catch { }

            var cliente = listaClientes.FirstOrDefault(c => c.Id == id);
            if (cliente == null)
            {
                TempData["ErrorMessage"] = "No se encontró el cliente seleccionado.";
                return RedirectToAction(nameof(Clientes));
            }

            var listaPedidos = new List<PedidoDto>();
            try
            {
                var mapaUsuarios = await ObtenerDiccionarioUsuariosAsync();
                var responsePedidos = await _httpClient.GetAsync("api/pedidos");
                if (responsePedidos.IsSuccessStatusCode)
                {
                    var content = await responsePedidos.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(content))
                    {
                        foreach (var item in doc.RootElement.EnumerateArray())
                        {
                            int idPedido = ObtenerIdPedido(item, "id", "idPedido");
                            int usuarioId = ObtenerIdPedido(item, "usuarioId", "UsuarioId");
                            decimal totalPedido = item.TryGetProperty("total", out var totalProp) ? totalProp.GetDecimal() : 0;
                            string metodoPago = item.TryGetProperty("metodoPago", out var pagoProp) ? pagoProp.GetString() ?? "EFECTIVO" : "EFECTIVO";
                            DateTime fechaPedido = item.TryGetProperty("fechaPedido", out var fechaProp) ? fechaProp.GetDateTime() : DateTime.Now;

                            string nombreCliente = ObtenerNombreCliente(mapaUsuarios, usuarioId);

                            listaPedidos.Add(new PedidoDto
                            {
                                Id = idPedido,
                                UsuarioId = usuarioId,
                                Cliente = nombreCliente,
                                Fecha = fechaPedido,
                                Total = totalPedido,
                                Estado = "PAGADO",
                                TipoEntrega = metodoPago
                            });
                        }
                    }
                }
            }
            catch { }

            var pedidosFiltrados = listaPedidos
                .Where(p => p.Id > 0 && p.UsuarioId == cliente.Id)
                .OrderByDescending(p => p.Fecha)
                .ToList();

            ViewBag.ClienteNombre = cliente.NombreCompleto;
            ViewBag.ClienteId = string.IsNullOrEmpty(cliente.NumeroCliente) || cliente.NumeroCliente == "CLI-000" 
                ? $"CLI-{cliente.Id:D3}" 
                : cliente.NumeroCliente;

            return View("~/Views/Pedidos/Index.cshtml", pedidosFiltrados);
        }

        // ==========================================
        // GESTIÓN DE EMPLEADOS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Empleados(string? busqueda)
        {
            string? rol = HttpContext.Session.GetString("RolSesion");
            if (rol != "Administrador")
            {
                return RedirectToAction("Index", "Empleado");
            }

            var empleados = new List<EmpleadoDto>();

            try
            {
                var response = await _httpClient.GetAsync("api/usuarios");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiUsuarios = JsonSerializer.Deserialize<List<EmpleadoDto>>(content, _jsonOptions);
                    if (apiUsuarios != null)
                    {
                        empleados = apiUsuarios.Where(e => 
                            (e.Rol == null || !e.Rol.Equals("Cliente", StringComparison.OrdinalIgnoreCase)) &&
                            (e.NombreCompleto == null || (!e.NombreCompleto.ToUpper().Contains("CLIENTE") && !e.NombreCompleto.ToUpper().Contains("JAVIER MILEI")))
                        ).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EN EMPLEADOS API]: {ex.Message}");
            }

            if (!string.IsNullOrEmpty(busqueda))
            {
                empleados = empleados.Where(e => (e.NombreCompleto != null && e.NombreCompleto.Contains(busqueda, StringComparison.OrdinalIgnoreCase)) ||
                                                 (e.Email != null && e.Email.Contains(busqueda, StringComparison.OrdinalIgnoreCase)) ||
                                                 (e.NumeroEmpleado != null && e.NumeroEmpleado.Contains(busqueda, StringComparison.OrdinalIgnoreCase)) ||
                                                 (e.Rol != null && e.Rol.Contains(busqueda, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            ViewBag.BusquedaActual = busqueda ?? "";

            return View("~/Views/Empleados/Index.cshtml", empleados);
        }

        [HttpGet]
        public IActionResult CrearEmpleado()
        {
            return View("~/Views/Empleados/Crear.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEmpleado(string Dni, string Email, string Nombre, string Apellido, string Usuario, string Rol, string Password, string? Telefono)
        {
            return RedirectToAction(nameof(Empleados));
        }

        [HttpGet]
        public IActionResult DetalleEmpleado(int id)
        {
            return View("~/Views/Empleados/Detalle.cshtml", new EmpleadoDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEmpleado(EmpleadoDto empleadoModificado)
        {
            return RedirectToAction(nameof(Empleados));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoEmpleado(int id, string nuevoEstado)
        {
            return RedirectToAction(nameof(Empleados));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEmpleado(int id)
        {
            return RedirectToAction(nameof(Empleados));
        }

        // ==========================================
        // GESTIÓN DE FACTURAS
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
                            if (pId > 0)
                            {
                                mapaPedidosUsuario[pId] = uId;
                            }
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
                            int idFactura = ObtenerIdPedido(elemento, "id");
                            int pedidoId = ObtenerIdPedido(elemento, "pedidoId", "PedidoId");
                            decimal totalFactura = elemento.TryGetProperty("total", out var totalProp) ? totalProp.GetDecimal() : 0;
                            DateTime fechaFactura = elemento.TryGetProperty("fecha", out var fechaProp) ? fechaProp.GetDateTime() : DateTime.Now;

                            int usuarioId = mapaPedidosUsuario.TryGetValue(pedidoId, out var uid) ? uid : 0;
                            string nombreCliente = ObtenerNombreCliente(mapaUsuarios, usuarioId);

                            listaFacturas.Add(new PedidoDto
                            {
                                Id = idFactura,
                                Cliente = nombreCliente,
                                Fecha = fechaFactura,
                                Total = totalFactura,
                                Estado = "PAGADO",
                                TipoEntrega = "EFECTIVO"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EN FACTURAS API]: {ex.Message}");
            }

            return View("~/Views/Facturas/Index.cshtml", listaFacturas.OrderByDescending(f => f.Fecha).ThenByDescending(f => f.Id).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> VerFactura(int id)
        {
            PedidoDto? pedido = null;

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
                            int pId = ObtenerIdPedido(itemP, "id");
                            int uId = ObtenerIdPedido(itemP, "usuarioId", "UsuarioId");
                            if (pId > 0) mapaPedidosUsuario[pId] = uId;
                        }
                    }
                }

                var response = await _httpClient.GetAsync("api/facturas");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(content))
                    {
                        foreach (var elemento in doc.RootElement.EnumerateArray())
                        {
                            int idFactura = elemento.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0;
                            if (idFactura == id)
                            {
                                decimal totalFactura = elemento.TryGetProperty("total", out var totalProp) ? totalProp.GetDecimal() : 0;
                                string tipo = elemento.TryGetProperty("tipo", out var tipoProp) ? tipoProp.GetString() ?? "B" : "B";
                                int numero = elemento.TryGetProperty("numero", out var numProp) ? numProp.GetInt32() : 0;
                                int pedidoId = elemento.TryGetProperty("pedidoId", out var pProp) ? pProp.GetInt32() : 0;
                                DateTime fechaFactura = elemento.TryGetProperty("fecha", out var fechaProp) ? fechaProp.GetDateTime() : DateTime.Now;

                                int usuarioId = mapaPedidosUsuario.TryGetValue(pedidoId, out var uid) ? uid : 0;
                                string nombreCliente = mapaUsuarios.TryGetValue(usuarioId, out var nombre) && !string.IsNullOrWhiteSpace(nombre) 
                                    ? nombre 
                                    : $"Cliente #{usuarioId}";

                                pedido = new PedidoDto
                                {
                                    Id = idFactura,
                                    Cliente = nombreCliente,
                                    Fecha = fechaFactura,
                                    Total = totalFactura,
                                    Estado = "APROBADO",
                                    TipoEntrega = "EFECTIVO"
                                };
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EN VER FACTURA API]: {ex.Message}");
            }

            if (pedido == null)
            {
                pedido = new PedidoDto { Id = id, Cliente = "Cliente Desconocido", Total = 0, Fecha = DateTime.Now, Estado = "APROBADO", TipoEntrega = "EFECTIVO" };
            }

            return View("~/Views/Facturas/Detalle.cshtml", pedido);
        }

        // ==========================================
        // CAMBIAR CONTRASEÑA
        // ==========================================

        [HttpGet]
        [ActionName("CambiarContrasena")]
        public IActionResult CambiarContrasena()
        {
            return View("~/Views/Administrador/CambiarContrasena.cshtml", new CambiarContrasenaViewModel());
        }

        [HttpPost]
        [ActionName("CambiarContrasena")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(CambiarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Administrador/CambiarContrasena.cshtml", model);
            }

            TempData["SuccessMessage"] = "Tu contraseña ha sido actualizada con éxito.";
            return RedirectToAction("CambiarContrasena");
        }
    }

}