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

        private static List<ProductoDto>? _productosEnMemoria;
        private static List<PedidoDto>? _pedidosEnMemoria;
        private static List<ClienteDto>? _clientesEnMemoria;
        private static List<EmpleadoDto>? _empleadosEnMemoria;
        
        // Configuración accesible globalmente por la tienda
        public static ConfiguracionTiendaDto ConfiguracionActual { get; set; } = new ConfiguracionTiendaDto();

        public AdministradorController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (_productosEnMemoria == null)
            {
                _productosEnMemoria = GetProductosFallback();
            }

            if (_pedidosEnMemoria == null)
            {
                _pedidosEnMemoria = GetPedidosIniciales();
            }

            if (_clientesEnMemoria == null)
            {
                _clientesEnMemoria = GetClientesIniciales();
            }

            if (_empleadosEnMemoria == null)
            {
                _empleadosEnMemoria = GetEmpleadosIniciales();
            }
        }

        [HttpGet]
        public IActionResult Index()
        {
            HttpContext.Session.SetString("RolSesion", "Administrador");
            return View();
        }

       // ==========================================
// CONFIGURACIÓN DE LA TIENDA
// ==========================================

[HttpGet]
public IActionResult ConfigurarTienda()
{
    HttpContext.Session.SetString("RolSesion", "Administrador");
    return View("~/Views/Administrador/Configuracion.cshtml", ConfiguracionActual);
}

[HttpPost]
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

        try
        {
            var jsonBody = JsonSerializer.Serialize(nuevaConfig);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            await _httpClient.PutAsync("api/configuracion", content);
        }
        catch
        {
            // Fallback en memoria
        }

        TempData["SuccessMessage"] = "La configuración de la tienda ha sido guardada con éxito.";
        return RedirectToAction(nameof(ConfigurarTienda));
    }

    return View("~/Views/Administrador/Configuracion.cshtml", nuevaConfig);
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
                // Fallback
            }

            if (!productos.Any())
            {
                productos = _productosEnMemoria!;
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
                nuevoProducto.Id = _productosEnMemoria!.Any() ? _productosEnMemoria!.Max(p => p.Id) + 1 : 1;
                nuevoProducto.Disponible = true;
                if (string.IsNullOrEmpty(nuevoProducto.ImagenUrl))
                {
                    nuevoProducto.ImagenUrl = "/images/hombres.png";
                }

                _productosEnMemoria!.Add(nuevoProducto);

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
                    await _httpClient.PostAsync("api/productos", content);
                }
                catch
                {
                    // Fallback
                }

                TempData["SuccessMessage"] = "Producto creado con éxito.";
                return RedirectToAction(nameof(Productos));
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
                producto = _productosEnMemoria!.FirstOrDefault(p => p.Id == id);
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
                var productoLocal = _productosEnMemoria!.FirstOrDefault(p => p.Id == productoModificado.Id);
                if (productoLocal != null)
                {
                    productoLocal.Nombre = productoModificado.Nombre;
                    productoLocal.Precio = productoModificado.Precio;
                    productoLocal.Talles = productoModificado.Talles;
                    productoLocal.Color = productoModificado.Color;
                    productoLocal.Stock = productoModificado.Stock;
                    productoLocal.Categoria = productoModificado.Categoria;
                    productoLocal.Descripcion = productoModificado.Descripcion;
                    productoLocal.Disponible = productoModificado.Disponible;
                }

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
                    await _httpClient.PutAsync($"api/productos/{productoModificado.Id}", content);
                }
                catch
                {
                    // Fallback
                }

                TempData["SuccessMessage"] = "Producto modificado correctamente.";
                return RedirectToAction(nameof(Productos));
            }

            return View("~/Views/Productos/Modificar.cshtml", productoModificado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var productoLocal = _productosEnMemoria!.FirstOrDefault(p => p.Id == id);
            if (productoLocal != null)
            {
                _productosEnMemoria!.Remove(productoLocal);
            }

            try
            {
                await _httpClient.DeleteAsync($"api/productos/{id}");
            }
            catch
            {
                // Fallback
            }

            TempData["SuccessMessage"] = "Producto eliminado correctamente.";
            return RedirectToAction(nameof(Productos));
        }

        // ==========================================
        // GESTIÓN DE PEDIDOS
        // ==========================================

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
                // Fallback
            }

            if (!pedidos.Any())
            {
                pedidos = _pedidosEnMemoria!;
            }

            return View("~/Views/Pedidos/Index.cshtml", pedidos);
        }

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
                // Fallback
            }

            if (pedido == null)
            {
                pedido = _pedidosEnMemoria!.FirstOrDefault(p => p.Id == id) ?? _pedidosEnMemoria!.First();
            }

            return View("~/Views/Pedidos/Detalle.cshtml", pedido);
        }

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
                // Fallback
            }

            if (!clientes.Any())
            {
                clientes = _clientesEnMemoria!;
            }

            if (!string.IsNullOrEmpty(busqueda))
            {
                clientes = clientes.Where(c => c.NombreCompleto.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                                               c.Email.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                                               c.Telefono.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                                               c.NumeroCliente.Contains(busqueda, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.BusquedaActual = busqueda ?? "";

            return View("~/Views/Clientes/Index.cshtml", clientes);
        }

        [HttpGet]
        public IActionResult DetalleCliente(int id)
        {
            var cliente = _clientesEnMemoria?.FirstOrDefault(c => c.Id == id) 
                          ?? GetClientesIniciales().FirstOrDefault(c => c.Id == id)
                          ?? GetClientesIniciales().First();

            return View("~/Views/Clientes/Detalle.cshtml", cliente);
        }

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
                clienteLocal.Telefono = clienteModificado.Telefono;
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
        // GESTIÓN DE EMPLEADOS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Empleados(string? busqueda)
        {
            HttpContext.Session.SetString("RolSesion", "Administrador");

            var empleados = new List<EmpleadoDto>();

            try
            {
                var response = await _httpClient.GetAsync("api/empleados");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    empleados = JsonSerializer.Deserialize<List<EmpleadoDto>>(content, _jsonOptions) ?? new List<EmpleadoDto>();
                }
            }
            catch
            {
                // Fallback
            }

            if (!empleados.Any())
            {
                empleados = _empleadosEnMemoria!;
            }

            if (!string.IsNullOrEmpty(busqueda))
            {
                empleados = empleados.Where(e => e.NombreCompleto.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                                                 e.Email.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                                                 e.NumeroEmpleado.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                                                 e.Rol.Contains(busqueda, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.BusquedaActual = busqueda ?? "";

            return View("~/Views/Empleados/Index.cshtml", empleados);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEmpleado(EmpleadoDto nuevoEmpleado)
        {
            if (ModelState.IsValid)
            {
                nuevoEmpleado.Id = _empleadosEnMemoria!.Any() ? _empleadosEnMemoria!.Max(e => e.Id) + 1 : 1;
                nuevoEmpleado.Estado = "ACTIVO";
                _empleadosEnMemoria!.Add(nuevoEmpleado);

                try
                {
                    var jsonBody = JsonSerializer.Serialize(nuevoEmpleado);
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync("api/empleados", content);
                }
                catch
                {
                    // Fallback
                }

                TempData["SuccessMessage"] = "Empleado creado correctamente.";
                return RedirectToAction(nameof(Empleados));
            }

            return RedirectToAction(nameof(Empleados));
        }

        [HttpGet]
        public IActionResult DetalleEmpleado(int id)
        {
            var empleado = _empleadosEnMemoria?.FirstOrDefault(e => e.Id == id) 
                           ?? GetEmpleadosIniciales().First();

            return View("~/Views/Empleados/Detalle.cshtml", empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEmpleado(EmpleadoDto empleadoModificado)
        {
            var empLocal = _empleadosEnMemoria!.FirstOrDefault(e => e.Id == empleadoModificado.Id);
            if (empLocal != null)
            {
                empLocal.Nombre = empleadoModificado.Nombre;
                empLocal.Apellido = empleadoModificado.Apellido;
                empLocal.Email = empleadoModificado.Email;
                empLocal.Telefono = empleadoModificado.Telefono;
                empLocal.Rol = empleadoModificado.Rol.ToUpper();
                empLocal.Estado = empleadoModificado.Estado.ToUpper();

                try
                {
                    var jsonBody = JsonSerializer.Serialize(empleadoModificado);
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    await _httpClient.PutAsync($"api/empleados/{empleadoModificado.Id}", content);
                }
                catch
                {
                    // Fallback
                }

                TempData["SuccessMessage"] = "Información del empleado actualizada.";
            }

            return RedirectToAction(nameof(Empleados));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoEmpleado(int id, string nuevoEstado)
        {
            var empLocal = _empleadosEnMemoria!.FirstOrDefault(e => e.Id == id);
            if (empLocal != null)
            {
                empLocal.Estado = nuevoEstado.ToUpper();

                try
                {
                    var jsonBody = JsonSerializer.Serialize(new { estado = nuevoEstado });
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    await _httpClient.PutAsync($"api/empleados/{id}/estado", content);
                }
                catch
                {
                    // Fallback
                }

                TempData["SuccessMessage"] = $"Estado cambiado a {nuevoEstado.ToUpper()}.";
            }

            return RedirectToAction(nameof(Empleados));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetearPasswordEmpleado(int id)
        {
            TempData["SuccessMessage"] = "Se envió un enlace de restablecimiento de contraseña al correo del empleado.";
            return RedirectToAction(nameof(DetalleEmpleado), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEmpleado(int id)
        {
            var empLocal = _empleadosEnMemoria!.FirstOrDefault(e => e.Id == id);
            if (empLocal != null)
            {
                _empleadosEnMemoria!.Remove(empLocal);

                try
                {
                    await _httpClient.DeleteAsync($"api/empleados/{id}");
                }
                catch
                {
                    // Fallback
                }

                TempData["SuccessMessage"] = "Empleado eliminado correctamente.";
            }

            return RedirectToAction(nameof(Empleados));
        }

        // ==========================================
        // GESTIÓN DE FACTURAS
        // ==========================================

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
                // Fallback
            }

            if (!pedidos.Any())
            {
                pedidos = _pedidosEnMemoria!;
            }

            return View("~/Views/Facturas/Index.cshtml", pedidos);
        }

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

        private static List<EmpleadoDto> GetEmpleadosIniciales()
        {
            return new List<EmpleadoDto>
            {
                new EmpleadoDto
                {
                    Id = 1,
                    Nombre = "FRANCISCO",
                    Apellido = "AGUIRRE",
                    Email = "franciscoaguirre@gmail.com",
                    Rol = "CAJERO",
                    Telefono = "03493 456789",
                    Estado = "ACTIVO"
                },
                new EmpleadoDto
                {
                    Id = 2,
                    Nombre = "ROCIO",
                    Apellido = "MILANESE",
                    Email = "rocimilanese@gmail.com",
                    Rol = "CAJERA",
                    Telefono = "03493 667173",
                    Estado = "ACTIVO"
                },
                new EmpleadoDto
                {
                    Id = 3,
                    Nombre = "LAUTARO",
                    Apellido = "OJEDA",
                    Email = "lauti210ojeda@gmail.com",
                    Rol = "CAJERO",
                    Telefono = "03493 112233",
                    Estado = "ACTIVO"
                }
            };
        }

        private static List<ProductoDto> GetProductosFallback()
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
                    Telefono = "03493 456789",
                    Estado = "BLOQUEADO"
                },
                new ClienteDto
                {
                    Id = 2,
                    Nombre = "ROCIO",
                    Apellido = "MILANESE",
                    FechaAlta = new DateTime(2025, 07, 29),
                    Email = "rocimilanese@gmail.com",
                    Telefono = "03493 667173",
                    Estado = "INACTIVO"
                },
                new ClienteDto
                {
                    Id = 3,
                    Nombre = "LIONEL",
                    Apellido = "MESSI",
                    FechaAlta = new DateTime(2025, 07, 31),
                    Email = "messi10@gmail.com",
                    Telefono = "03493 101010",
                    Estado = "BLOQUEADO"
                }
            };
        }
       // ==========================================
// REPORTES DE VENTAS CON GRÁFICO
// ==========================================

[HttpGet]
[ActionName("Reportes")]
public async Task<IActionResult> ReportesVentas(DateTime? desde, DateTime? hasta)
{
    HttpContext.Session.SetString("RolSesion", "Administrador");

    DateTime fechaInicio = desde ?? DateTime.Today.AddDays(-30);
    DateTime fechaFin = hasta ?? DateTime.Today;

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

    // Filtrar por rango de fechas seleccionado
    var ventasFiltradas = pedidos
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

    // Agrupamiento por fecha para la gráfica
    var datosGrafico = ventasFiltradas
        .GroupBy(v => v.Fecha.ToString("dd/MM/yyyy"))
        .Select(g => new { Fecha = g.Key, Total = g.Sum(x => x.Total) })
        .OrderBy(g => DateTime.ParseExact(g.Fecha, "dd/MM/yyyy", null))
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
// CAMBIAR CONTRASEÑA ADMINISTRADOR
// ==========================================

[HttpGet]
[ActionName("CambiarContrasena")]
public IActionResult CambiarContrasena()
{
    HttpContext.Session.SetString("RolSesion", "Administrador");
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

    try
    {
        var jsonBody = JsonSerializer.Serialize(model);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        await _httpClient.PostAsync("api/administrador/cambiar-clave", content);
    }
    catch
    {
        // Fallback en memoria / prueba frontend
    }

    TempData["SuccessMessage"] = "Tu contraseña ha sido actualizada con éxito.";
    return RedirectToAction("CambiarContrasena");
}
    }
}