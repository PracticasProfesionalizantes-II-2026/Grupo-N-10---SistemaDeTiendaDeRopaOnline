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

        // Listas estáticas en memoria para preservar modificaciones locales durante la ejecución
        private static List<PedidoDto>? _pedidosEnMemoria;
        private static List<ClienteDto>? _clientesEnMemoria;

        public AdministradorController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (_pedidosEnMemoria == null)
            {
                _pedidosEnMemoria = GetPedidosIniciales();
            }

            if (_clientesEnMemoria == null)
            {
                _clientesEnMemoria = GetClientesIniciales();
            }
        }

        // ==========================================
        // PANEL PRINCIPAL
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            HttpContext.Session.SetString("RolSesion", "Administrador");
            return View();
        }

        // ==========================================
        // GESTIÓN DE PRODUCTOS
        // ==========================================

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
    }
}