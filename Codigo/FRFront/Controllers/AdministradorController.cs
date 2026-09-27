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

     

        [HttpGet]
        public async Task<IActionResult> Productos(string? categoria, string? busqueda)
        {
            HttpContext.Session.SetString("RolSesion", "Administrador");

            var productos = new List<ProductoDto>();

            try
            {
                var response = await _httpClient.GetAsync("api/productos");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    productos = JsonSerializer.Deserialize<List<ProductoDto>>(content, _jsonOptions) ?? new List<ProductoDto>();
                }
            }
            catch
            {
                // Fallback local
            }

            if (!productos.Any())
            {
                productos = GetProductosFallback();
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
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(nuevoProducto.Nombre ?? ""), nameof(nuevoProducto.Nombre));
                content.Add(new StringContent(nuevoProducto.Precio.ToString()), nameof(nuevoProducto.Precio));
                content.Add(new StringContent(nuevoProducto.Talles ?? ""), nameof(nuevoProducto.Talles));
                content.Add(new StringContent(nuevoProducto.Color ?? ""), nameof(nuevoProducto.Color));
                content.Add(new StringContent(nuevoProducto.Stock.ToString()), nameof(nuevoProducto.Stock));
                content.Add(new StringContent(nuevoProducto.Categoria ?? ""), nameof(nuevoProducto.Categoria));
                content.Add(new StringContent(nuevoProducto.Descripcion ?? ""), nameof(nuevoProducto.Descripcion));

                if (imagenFile != null && imagenFile.Length > 0)
                {
                    var streamContent = new StreamContent(imagenFile.OpenReadStream());
                    content.Add(streamContent, "imagenFile", imagenFile.FileName);
                }

                try
                {
                    var response = await _httpClient.PostAsync("api/productos", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Producto creado con éxito.";
                        return RedirectToAction(nameof(Productos));
                    }

                    ModelState.AddModelError(string.Empty, "Error al guardar el producto.");
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "No se pudo conectar con el servidor.");
                }
            }

            return View("~/Views/Productos/Crear.cshtml", nuevoProducto);
        }

        [HttpGet]
        public async Task<IActionResult> EditarProducto(int id)
        {
            ProductoDto? producto = null;

            try
            {
                var response = await _httpClient.GetAsync($"api/productos/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    producto = JsonSerializer.Deserialize<ProductoDto>(content, _jsonOptions);
                }
            }
            catch
            {
                // Fallback
            }

            if (producto == null)
            {
                producto = GetProductosFallback().FirstOrDefault(p => p.Id == id);
            }

            if (producto == null)
            {
                return RedirectToAction(nameof(Productos));
            }

            return View("~/Views/Productos/Modificar.cshtml", producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProducto(ProductoDto productoModificado, IFormFile? imagenFile)
        {
            if (ModelState.IsValid)
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(productoModificado.Id.ToString()), nameof(productoModificado.Id));
                content.Add(new StringContent(productoModificado.Nombre ?? ""), nameof(productoModificado.Nombre));
                content.Add(new StringContent(productoModificado.Precio.ToString()), nameof(productoModificado.Precio));
                content.Add(new StringContent(productoModificado.Talles ?? ""), nameof(productoModificado.Talles));
                content.Add(new StringContent(productoModificado.Color ?? ""), nameof(productoModificado.Color));
                content.Add(new StringContent(productoModificado.Stock.ToString()), nameof(productoModificado.Stock));
                content.Add(new StringContent(productoModificado.Categoria ?? ""), nameof(productoModificado.Categoria));
                content.Add(new StringContent(productoModificado.Descripcion ?? ""), nameof(productoModificado.Descripcion));
                content.Add(new StringContent(productoModificado.Disponible.ToString()), nameof(productoModificado.Disponible));

                if (imagenFile != null && imagenFile.Length > 0)
                {
                    var streamContent = new StreamContent(imagenFile.OpenReadStream());
                    content.Add(streamContent, "imagenFile", imagenFile.FileName);
                }

                try
                {
                    var response = await _httpClient.PutAsync($"api/productos/{productoModificado.Id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Producto modificado correctamente.";
                        return RedirectToAction(nameof(Productos));
                    }

                    ModelState.AddModelError(string.Empty, "Error al actualizar el producto.");
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "No se pudo comunicar con el backend.");
                }
            }

            return View("~/Views/Productos/Modificar.cshtml", productoModificado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/productos/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Producto eliminado correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el producto.";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Error de conexión al eliminar.";
            }

            return RedirectToAction(nameof(Productos));
        }

        // ==========================================
        // GESTIÓN DE PEDIDOS
        // ==========================================

        // GET: /Administrador/Pedidos
        [HttpGet]
        public async Task<IActionResult> Pedidos()
        {
            HttpContext.Session.SetString("RolSesion", "Administrador");

            var pedidos = new List<PedidoDto>();

            try
            {
                var response = await _httpClient.GetAsync("api/pedidos");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    pedidos = JsonSerializer.Deserialize<List<PedidoDto>>(content, _jsonOptions) ?? new List<PedidoDto>();
                }
            }
            catch
            {
                // API no disponible
            }

            if (!pedidos.Any())
            {
                pedidos = _pedidosEnMemoria!;
            }

            return View("~/Views/Pedidos/Index.cshtml", pedidos);
        }

        // GET: /Administrador/DetallePedido/1
        [HttpGet]
        public async Task<IActionResult> DetallePedido(int id)
        {
            PedidoDto? pedido = null;

            try
            {
                var response = await _httpClient.GetAsync($"api/pedidos/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    pedido = JsonSerializer.Deserialize<PedidoDto>(content, _jsonOptions);
                }
            }
            catch
            {
                // API no disponible
            }

            if (pedido == null)
            {
                pedido = _pedidosEnMemoria!.FirstOrDefault(p => p.Id == id) ?? _pedidosEnMemoria!.First();
            }

            return View("~/Views/Pedidos/Detalle.cshtml", pedido);
        }

        // POST: /Administrador/ActualizarEstadoPedido
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstadoPedido(int id, string nuevoEstado)
        {
            var estadosValidos = new[] { "CONFIRMADO", "EN CAMINO", "ENTREGADO", "CANCELADO" };
            if (!string.IsNullOrEmpty(nuevoEstado) && estadosValidos.Contains(nuevoEstado.ToUpper()))
            {
                var pedidoLocal = _pedidosEnMemoria!.FirstOrDefault(p => p.Id == id);
                if (pedidoLocal != null)
                {
                    pedidoLocal.Estado = nuevoEstado.ToUpper();
                }

                try
                {
                    var jsonBody = JsonSerializer.Serialize(new { estado = nuevoEstado });
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    await _httpClient.PutAsync($"api/pedidos/{id}/estado", content);
                }
                catch
                {
                    // Fallback
                }

                TempData["SuccessMessage"] = "Estado del pedido actualizado correctamente.";
            }

            return RedirectToAction(nameof(Pedidos));
        }

        // ==========================================
        // GESTIÓN DE CLIENTES
        // ==========================================

        // GET: /Administrador/Clientes
        [HttpGet]
        public async Task<IActionResult> Clientes(string? busqueda)
        // ==========================================
        // CONFIGURACIÓN DE LA TIENDA
        // ==========================================

        [HttpGet("Administrador/ConfigurarTienda")]
        [HttpGet("Administrador/Configuracion")]
        [ActionName("ConfigurarTienda")]
        public IActionResult ConfigurarTienda()
        {
            HttpContext.Session.SetString("RolSesion", "Administrador");

            var clientes = new List<ClienteDto>();

            try
            {
                var response = await _httpClient.GetAsync("api/clientes");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    clientes = JsonSerializer.Deserialize<List<ClienteDto>>(content, _jsonOptions) ?? new List<ClienteDto>();
                }
            }
            catch
            {
                // Fallback local
            }

            if (!clientes.Any())
            {
                clientes = _clientesEnMemoria!;
            }

            if (!string.IsNullOrEmpty(busqueda))
            {
                clientes = clientes.Where(c => c.NombreCompleto.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                                               c.Email.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                                               c.NumeroCliente.Contains(busqueda, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.BusquedaActual = busqueda ?? "";

            return View("~/Views/Clientes/Index.cshtml", clientes);
        }

        // GET: /Administrador/DetalleCliente/1
        [HttpGet]
        public IActionResult DetalleCliente(int id)
        {
            var cliente = _clientesEnMemoria?.FirstOrDefault(c => c.Id == id) 
                          ?? GetClientesIniciales().First();

            return View("~/Views/Clientes/Detalle.cshtml", cliente);
        }

        // POST: /Administrador/EditarCliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCliente(ClienteDto clienteModificado)
        {
            var clienteLocal = _clientesEnMemoria!.FirstOrDefault(c => c.Id == clienteModificado.Id);
            if (clienteLocal != null)
            {
                clienteLocal.Nombre = clienteModificado.Nombre;
                clienteLocal.Apellido = clienteModificado.Apellido;
                clienteLocal.Email = clienteModificado.Email;
                clienteLocal.Estado = clienteModificado.Estado.ToUpper();

                try
                {
                    var jsonBody = JsonSerializer.Serialize(clienteModificado);
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    await _httpClient.PutAsync($"api/clientes/{clienteModificado.Id}", content);
                }
                catch
                {
                    // Fallback
                }
            }

            return RedirectToAction(nameof(Clientes));
        }

        // GET: /Administrador/HistorialCliente/1
        [HttpGet]
        public IActionResult HistorialCliente(int id)
        {
            var cliente = _clientesEnMemoria?.FirstOrDefault(c => c.Id == id) 
                          ?? GetClientesIniciales().First();

            var pedidosCliente = _pedidosEnMemoria?
                .Where(p => p.Cliente.Contains(cliente.Nombre, StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<PedidoDto>();

            ViewBag.ClienteNombre = cliente.NombreCompleto;
            ViewBag.ClienteId = cliente.NumeroCliente;

            return View("~/Views/Pedidos/Index.cshtml", pedidosCliente);
        }

        // POST: /Administrador/CambiarEstadoCliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoCliente(int id, string nuevoEstado)
        {
            var clienteLocal = _clientesEnMemoria!.FirstOrDefault(c => c.Id == id);
            if (clienteLocal != null)
            {
                clienteLocal.Estado = nuevoEstado.ToUpper();

                try
                {
                    var jsonBody = JsonSerializer.Serialize(new { estado = nuevoEstado });
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    await _httpClient.PutAsync($"api/clientes/{id}/estado", content);
                }
                catch
                {
                    // Fallback
                }
            }

            return RedirectToAction(nameof(Clientes));
        }

        // POST: /Administrador/EliminarCliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var clienteLocal = _clientesEnMemoria!.FirstOrDefault(c => c.Id == id);
            if (clienteLocal != null)
            {
                _clientesEnMemoria!.Remove(clienteLocal);

                try
                {
                    await _httpClient.DeleteAsync($"api/clientes/{id}");
                }
                catch
                {
                    // Fallback
                }
            }

            return RedirectToAction(nameof(Clientes));
        }

        // ==========================================
        // GESTIÓN DE FACTURAS
        // ==========================================

        // GET: /Administrador/Facturas
        [HttpGet]
        public async Task<IActionResult> Facturas()
        {
            HttpContext.Session.SetString("RolSesion", "Administrador");

            var pedidos = new List<PedidoDto>();

            try
            {
                var response = await _httpClient.GetAsync("api/pedidos");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    pedidos = JsonSerializer.Deserialize<List<PedidoDto>>(content, _jsonOptions) ?? new List<PedidoDto>();
                }
            }
            catch
            {
                // Fallback local
            }

            if (!pedidos.Any())
            {
                pedidos = _pedidosEnMemoria!;
            }

            return View("~/Views/Facturas/Index.cshtml", pedidos);
        }

        // GET: /Administrador/VerFactura/1
        [HttpGet]
        public IActionResult VerFactura(int id)
        {
            var pedido = _pedidosEnMemoria?.FirstOrDefault(p => p.Id == id) 
                         ?? GetPedidosIniciales().First();

            return View("~/Views/Facturas/Detalle.cshtml", pedido);
        }

        // ==========================================
        // DATOS DE RESPALDO (FALLBACK)
        // ==========================================

        private List<ProductoDto> GetProductosFallback()
        {
            return new List<ProductoDto>
            {
                new ProductoDto { Id = 1, Nombre = "BUZO VCV", Precio = 80000, Talles = "M/L", Color = "BEIGE", Stock = 10, Categoria = "abrigos", ImagenUrl = "/images/hombres.png", Disponible = true },
                new ProductoDto { Id = 2, Nombre = "REMERA ACTIVE", Precio = 20000, Talles = "XS/S/M", Color = "NEGRO", Stock = 14, Categoria = "remeras", ImagenUrl = "/images/mujeres.png", Disponible = true },
                new ProductoDto { Id = 3, Nombre = "HOODIE URBAN", Precio = 65000, Talles = "L/XL", Color = "VERDE", Stock = 8, Categoria = "abrigos", ImagenUrl = "/images/hombres2.png", Disponible = true },
                new ProductoDto { Id = 4, Nombre = "TOP OVERSIDE", Precio = 25000, Talles = "S/M", Color = "BLANCO", Stock = 5, Categoria = "remeras", ImagenUrl = "/images/mujeres2.png", Disponible = true },
                new ProductoDto { Id = 5, Nombre = "PANTALON CARGO", Precio = 55000, Talles = "38/40/42", Color = "NEGRO", Stock = 12, Categoria = "pantalones", ImagenUrl = "/images/hombres3.png", Disponible = true }
            };
        }

        private static List<PedidoDto> GetPedidosIniciales()
        {
            return new List<PedidoDto>
            {
                new PedidoDto
                {
                    Id = 1,
                    Cliente = "FRANCISCO AGUIRRE",
                    Fecha = new DateTime(2025, 07, 25),
                    Total = 26890,
                    Estado = "ENTREGADO",
                    TipoEntrega = "RETIRO LOCAL",
                    Detalle = new List<DetallePedidoDto>
                    {
                        new DetallePedidoDto { ProductoId = 1, ProductoNombre = "Pollera maite", Cantidad = 1, PrecioUnitario = 25000 }
                    }
                },
                new PedidoDto
                {
                    Id = 2,
                    Cliente = "ROCIO MILANESE",
                    Fecha = new DateTime(2025, 07, 29),
                    Total = 100000,
                    Estado = "EN CAMINO",
                    TipoEntrega = "ENVÍO A DOMICILIO",
                    Detalle = new List<DetallePedidoDto>
                    {
                        new DetallePedidoDto { ProductoId = 2, ProductoNombre = "REMERA ACTIVE", Cantidad = 2, PrecioUnitario = 20000 },
                        new DetallePedidoDto { ProductoId = 3, ProductoNombre = "HOODIE URBAN", Cantidad = 1, PrecioUnitario = 60000 }
                    }
                },
                new PedidoDto
                {
                    Id = 3,
                    Cliente = "LIONEL MESSI",
                    Fecha = new DateTime(2025, 07, 31),
                    Total = 60000,
                    Estado = "CONFIRMADO",
                    TipoEntrega = "RETIRO LOCAL",
                    Detalle = new List<DetallePedidoDto>
                    {
                        new DetallePedidoDto { ProductoId = 3, ProductoNombre = "HOODIE URBAN", Cantidad = 1, PrecioUnitario = 60000 }
                    }
                }
            };
        }

        private static List<ClienteDto> GetClientesIniciales()
        {
            return new List<ClienteDto>
            {
                new ClienteDto
                {
                    Id = 1,
                    Nombre = "FRANCISCO",
                    Apellido = "AGUIRRE",
                    FechaAlta = new DateTime(2025, 07, 25),
                    Email = "franciscoaguirre@gmail.com",
                    Estado = "ACTIVO"
                },
                new ClienteDto
                {
                    Id = 2,
                    Nombre = "ROCIO",
                    Apellido = "MILANESE",
                    FechaAlta = new DateTime(2025, 07, 29),
                    Email = "rocimilanese@gmail.com",
                    Estado = "INACTIVO"
                },
                new ClienteDto
                {
                    Id = 3,
                    Nombre = "LIONEL",
                    Apellido = "MESSI",
                    FechaAlta = new DateTime(2025, 07, 31),
                    Email = "messi10@gmail.com",
                    Estado = "BLOQUEADO"
                }
            };
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
                            if (estadoPedido == "5")
                            {
                                estadoPedido = "PAGADO";
                            }
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
                var response = await _httpClient.PatchAsync($"api/pedidos/{id}/estado", estadoRequest);
                TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] = response.IsSuccessStatusCode
                    ? "El estado del pedido se actualizó correctamente."
                    : "No se pudo actualizar el estado del pedido.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR AL ACTUALIZAR ESTADO DEL PEDIDO]: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al actualizar el pedido.";
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
            try
            {
                var request = new
                {
                    Nombre = clienteModificado.Nombre,
                    Apellido = clienteModificado.Apellido,
                    Email = clienteModificado.Email,
                    Rol = 5,
                    Telefono = clienteModificado.Telefono,
                    IdiomaPreferido = "es",
                    FotoPerfil = (string?)null,
                    Activo = clienteModificado.Estado.Equals("ACTIVO", StringComparison.OrdinalIgnoreCase),
                    EmpresaId = (int?)null
                };
                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/usuarios/{clienteModificado.Id}", content);
                TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] = response.IsSuccessStatusCode
                    ? "Los datos del cliente se guardaron correctamente."
                    : "No se pudieron guardar los datos del cliente.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR AL EDITAR CLIENTE]: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al guardar el cliente.";
            }

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
                        foreach (var usuario in apiUsuarios)
                        {
                            if (usuario.Id <= 0 && usuario.IdUsuario > 0)
                            {
                                usuario.Id = usuario.IdUsuario;
                            }

                            var rolApi = !string.IsNullOrWhiteSpace(usuario.TipoUsuario)
                                ? usuario.TipoUsuario
                                : usuario.Rol;
                            usuario.Rol = rolApi.Trim();
                        }

                        empleados = apiUsuarios
                            .Where(e => e.Rol.Equals("Empleado", StringComparison.OrdinalIgnoreCase) ||
                                        e.Rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                            .ToList();
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
            try
            {
                var request = new
                {
                    Dni,
                    Nombre,
                    Apellido,
                    Email,
                    Password,
                    Rol = Rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ? 3 : 4,
                    Telefono,
                    IdiomaPreferido = "es",
                    FotoPerfil = (string?)null,
                    EmpresaId = (int?)null,
                    Activo = true
                };
                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/usuarios", content);
                TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] = response.IsSuccessStatusCode
                    ? "El empleado se creó correctamente."
                    : "No se pudo crear el empleado.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR AL CREAR EMPLEADO]: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al crear el empleado.";
            }

            return RedirectToAction(nameof(Empleados));
        }

        [HttpGet]
        public async Task<IActionResult> DetalleEmpleado(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/usuarios/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var usuario = JsonSerializer.Deserialize<EmpleadoDto>(content, _jsonOptions);
                    if (usuario != null)
                    {
                        if (usuario.Id <= 0 && usuario.IdUsuario > 0)
                        {
                            usuario.Id = usuario.IdUsuario;
                        }

                        usuario.Rol = !string.IsNullOrWhiteSpace(usuario.TipoUsuario)
                            ? usuario.TipoUsuario.Trim()
                            : usuario.Rol.Trim();
                        usuario.Estado = usuario.Activo ? "ACTIVO" : "BLOQUEADO";

                        return View("~/Views/Empleados/Detalle.cshtml", usuario);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR AL CARGAR DETALLE DE EMPLEADO]: {ex.Message}");
            }

            TempData["ErrorMessage"] = "No se encontró el usuario seleccionado.";
            return RedirectToAction(nameof(Empleados));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEmpleado(EmpleadoDto empleadoModificado)
        {
            try
            {
                var rol = empleadoModificado.Rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase)
                    ? 3
                    : 4;
                var activo = !empleadoModificado.Estado.Equals("BLOQUEADO", StringComparison.OrdinalIgnoreCase);
                var request = new
                {
                    Nombre = empleadoModificado.Nombre,
                    Apellido = empleadoModificado.Apellido,
                    Email = empleadoModificado.Email,
                    Rol = rol,
                    Telefono = empleadoModificado.Telefono,
                    IdiomaPreferido = "es",
                    FotoPerfil = (string?)null,
                    Activo = activo,
                    EmpresaId = (int?)null
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");
                var response = await _httpClient.PutAsync($"api/usuarios/{empleadoModificado.Id}", content);

                TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] = response.IsSuccessStatusCode
                    ? "Los datos del empleado se guardaron correctamente."
                    : "No se pudieron guardar los datos del empleado.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR AL EDITAR EMPLEADO]: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al guardar los datos del empleado.";
            }

            return RedirectToAction(nameof(Empleados));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoEmpleado(int id, string nuevoEstado)
        {
            try
            {
                var usuarioResponse = await _httpClient.GetAsync($"api/usuarios/{id}");
                if (!usuarioResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "No se encontró el usuario.";
                    return RedirectToAction(nameof(Empleados));
                }

                var usuario = JsonSerializer.Deserialize<EmpleadoDto>(await usuarioResponse.Content.ReadAsStringAsync(), _jsonOptions)!;
                var rol = usuario.TipoUsuario.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ? 3 : 4;
                var request = new
                {
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Email = usuario.Email,
                    Rol = rol,
                    Telefono = usuario.Telefono,
                    IdiomaPreferido = "es",
                    FotoPerfil = (string?)null,
                    Activo = !nuevoEstado.Equals("BLOQUEADO", StringComparison.OrdinalIgnoreCase),
                    EmpresaId = (int?)null
                };
                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/usuarios/{id}", content);
                TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] = response.IsSuccessStatusCode
                    ? "El estado del usuario se actualizó correctamente."
                    : "No se pudo actualizar el estado del usuario.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR AL CAMBIAR ESTADO DEL EMPLEADO]: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al actualizar el estado.";
            }

            return RedirectToAction(nameof(Empleados));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEmpleado(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/usuarios/{id}");
                TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] = response.IsSuccessStatusCode
                    ? "El usuario se eliminó correctamente."
                    : "No se pudo eliminar el usuario.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR AL ELIMINAR EMPLEADO]: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar el usuario.";
            }

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