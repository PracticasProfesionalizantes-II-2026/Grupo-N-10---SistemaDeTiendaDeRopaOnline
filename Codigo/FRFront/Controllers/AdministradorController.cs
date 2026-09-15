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

        public static List<ProductoDto> ProductosEnMemoria { get; set; } = GetProductosFallback();
        public static List<PedidoDto> PedidosEnMemoria { get; set; } = GetPedidosIniciales();
        public static List<ClienteDto> ClientesEnMemoria { get; set; } = GetClientesIniciales();
        public static List<EmpleadoDto> EmpleadosEnMemoria { get; set; } = GetEmpleadosIniciales();
        
        public static ConfiguracionTiendaDto ConfiguracionActual { get; set; } = new ConfiguracionTiendaDto();

        public AdministradorController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public static void RegistrarNuevoPedido(PedidoDto nuevoPedido, string dni, string email, string telefono)
        {
            PedidosEnMemoria.Insert(0, nuevoPedido);

            var clienteExistente = ClientesEnMemoria.FirstOrDefault(c => 
                (!string.IsNullOrEmpty(c.Dni) && c.Dni == dni) || 
                c.NombreCompleto.Equals(nuevoPedido.Cliente, StringComparison.OrdinalIgnoreCase));

            if (clienteExistente == null)
            {
                string[] partesNombre = nuevoPedido.Cliente.Split(' ');
                string nom = partesNombre.Length > 0 ? partesNombre[0] : nuevoPedido.Cliente;
                string ape = partesNombre.Length > 1 ? string.Join(" ", partesNombre.Skip(1)) : "";

                ClientesEnMemoria.Add(new ClienteDto
                {
                    Id = ClientesEnMemoria.Any() ? ClientesEnMemoria.Max(c => c.Id) + 1 : 1,
                    Dni = string.IsNullOrEmpty(dni) ? "S/D" : dni,
                    Nombre = nom,
                    Apellido = ape,
                    Email = email,
                    Telefono = telefono,
                    FechaAlta = DateTime.Now,
                    Estado = "ACTIVO"
                });
            }
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
        public IActionResult ReportesVentas(DateTime? desde, DateTime? hasta)
        {
            string? rol = HttpContext.Session.GetString("RolSesion");
            if (rol != "Administrador")
            {
                return RedirectToAction("Index", "Empleado");
            }

            DateTime fechaInicio = desde ?? DateTime.Today.AddDays(-30);
            DateTime fechaFin = hasta ?? DateTime.Today.AddDays(1).AddSeconds(-1);

            var ventasFiltradas = PedidosEnMemoria
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
            var productos = ProductosEnMemoria;

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
                nuevoProducto.Id = ProductosEnMemoria.Any() ? ProductosEnMemoria.Max(p => p.Id) + 1 : 1;
                nuevoProducto.Disponible = true;
                if (string.IsNullOrEmpty(nuevoProducto.ImagenUrl))
                {
                    nuevoProducto.ImagenUrl = "/images/hombres.png";
                }

                ProductosEnMemoria.Add(nuevoProducto);

                TempData["SuccessMessage"] = "Producto creado con éxito.";
                return RedirectToAction(nameof(Productos));
            }

            return View("~/Views/Productos/Crear.cshtml", nuevoProducto);
        }

        [HttpGet]
        public async Task<IActionResult> EditarProducto(int id)
        {
            var producto = ProductosEnMemoria.FirstOrDefault(p => p.Id == id);
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
                var productoLocal = ProductosEnMemoria.FirstOrDefault(p => p.Id == productoModificado.Id);
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

                TempData["SuccessMessage"] = "Producto modificado correctamente.";
                return RedirectToAction(nameof(Productos));
            }

            return View("~/Views/Productos/Modificar.cshtml", productoModificado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var productoLocal = ProductosEnMemoria.FirstOrDefault(p => p.Id == id);
            if (productoLocal != null)
            {
                ProductosEnMemoria.Remove(productoLocal);
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
            return View("~/Views/Pedidos/Index.cshtml", PedidosEnMemoria);
        }

        [HttpGet]
        public async Task<IActionResult> DetallePedido(int id)
        {
            var pedido = PedidosEnMemoria.FirstOrDefault(p => p.Id == id) ?? PedidosEnMemoria.First();
            return View("~/Views/Pedidos/Detalle.cshtml", pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstadoPedido(int id, string nuevoEstado)
        {
            var estadosValidos = new[] { "CONFIRMADO", "EN CAMINO", "ENTREGADO", "CANCELADO" };
            if (!string.IsNullOrEmpty(nuevoEstado) && estadosValidos.Contains(nuevoEstado.ToUpper()))
            {
                var pedidoLocal = PedidosEnMemoria.FirstOrDefault(p => p.Id == id);
                if (pedidoLocal != null)
                {
                    pedidoLocal.Estado = nuevoEstado.ToUpper();
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
            var clientes = ClientesEnMemoria;

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
        public IActionResult DetalleCliente(int id)
        {
            var cliente = ClientesEnMemoria.FirstOrDefault(c => c.Id == id) ?? ClientesEnMemoria.First();
            return View("~/Views/Clientes/Detalle.cshtml", cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCliente(ClienteDto clienteModificado)
        {
            var clienteLocal = ClientesEnMemoria.FirstOrDefault(c => c.Id == clienteModificado.Id);
            if (clienteLocal != null)
            {
                clienteLocal.Dni = clienteModificado.Dni;
                clienteLocal.Nombre = clienteModificado.Nombre;
                clienteLocal.Apellido = clienteModificado.Apellido;
                clienteLocal.Email = clienteModificado.Email;
                clienteLocal.Telefono = clienteModificado.Telefono;
                clienteLocal.Estado = clienteModificado.Estado.ToUpper();
            }

            return RedirectToAction(nameof(Clientes));
        }

        [HttpGet]
        public IActionResult HistorialCliente(int id)
        {
            var cliente = ClientesEnMemoria.FirstOrDefault(c => c.Id == id) ?? ClientesEnMemoria.First();

            var pedidosCliente = PedidosEnMemoria
                .Where(p => p.Cliente.Contains(cliente.Nombre, StringComparison.OrdinalIgnoreCase) || 
                            p.Cliente.Contains(cliente.Apellido, StringComparison.OrdinalIgnoreCase))
                .ToList();

            ViewBag.ClienteNombre = cliente.NombreCompleto;
            ViewBag.ClienteId = cliente.NumeroCliente;

            return View("~/Views/Pedidos/Index.cshtml", pedidosCliente);
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

            var empleados = EmpleadosEnMemoria;

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
            string? rol = HttpContext.Session.GetString("RolSesion");
            if (rol != "Administrador")
            {
                return RedirectToAction("Index", "Empleado");
            }

            return View("~/Views/Empleados/Crear.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEmpleado(string Dni, string Email, string Nombre, string Apellido, string Usuario, string Rol, string Password, string? Telefono)
        {
            if (!string.IsNullOrEmpty(Nombre) && !string.IsNullOrEmpty(Apellido) && !string.IsNullOrEmpty(Email))
            {
                int nuevoId = EmpleadosEnMemoria.Any() ? EmpleadosEnMemoria.Max(e => e.Id) + 1 : 1;

                var nuevoEmpleado = new EmpleadoDto
                {
                    Id = nuevoId,
                    Nombre = Nombre.ToUpper(),
                    Apellido = Apellido.ToUpper(),
                    Email = Email.ToLower(),
                    Telefono = string.IsNullOrEmpty(Telefono) ? "Sin registrar" : Telefono,
                    Rol = string.IsNullOrEmpty(Rol) ? "CAJERO" : Rol.ToUpper(),
                    Estado = "ACTIVO"
                };

                EmpleadosEnMemoria.Add(nuevoEmpleado);

                TempData["SuccessMessage"] = $"Empleado {Nombre.ToUpper()} {Apellido.ToUpper()} registrado correctamente.";
                return RedirectToAction(nameof(Empleados));
            }

            return View("~/Views/Empleados/Crear.cshtml");
        }

        [HttpGet]
        public IActionResult DetalleEmpleado(int id)
        {
            var empleado = EmpleadosEnMemoria.FirstOrDefault(e => e.Id == id) ?? EmpleadosEnMemoria.First();
            return View("~/Views/Empleados/Detalle.cshtml", empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEmpleado(EmpleadoDto empleadoModificado)
        {
            var empLocal = EmpleadosEnMemoria.FirstOrDefault(e => e.Id == empleadoModificado.Id);
            if (empLocal != null)
            {
                empLocal.Nombre = empleadoModificado.Nombre;
                empLocal.Apellido = empleadoModificado.Apellido;
                empLocal.Email = empleadoModificado.Email;
                empLocal.Telefono = empleadoModificado.Telefono;
                empLocal.Rol = empleadoModificado.Rol?.ToUpper();
                empLocal.Estado = empleadoModificado.Estado?.ToUpper();

                TempData["SuccessMessage"] = "Información del empleado actualizada.";
            }

            return RedirectToAction(nameof(Empleados));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoEmpleado(int id, string nuevoEstado)
        {
            var empLocal = EmpleadosEnMemoria.FirstOrDefault(e => e.Id == id);
            if (empLocal != null)
            {
                empLocal.Estado = nuevoEstado.ToUpper();
                TempData["SuccessMessage"] = $"Estado cambiado a {nuevoEstado.ToUpper()}.";
            }

            return RedirectToAction(nameof(Empleados));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEmpleado(int id)
        {
            var empLocal = EmpleadosEnMemoria.FirstOrDefault(e => e.Id == id);
            if (empLocal != null)
            {
                EmpleadosEnMemoria.Remove(empLocal);
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
            return View("~/Views/Facturas/Index.cshtml", PedidosEnMemoria);
        }

        [HttpGet]
        public IActionResult VerFactura(int id)
        {
            var pedido = PedidosEnMemoria.FirstOrDefault(p => p.Id == id) ?? PedidosEnMemoria.First();
            return View("~/Views/Facturas/Detalle.cshtml", pedido);
        }

        // ==========================================
        // CAMBIAR CONTRASEÑA ADMINISTRADOR
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

        // ==========================================
        // DATOS DE RESPALDO (INITIALIZERS)
        // ==========================================

        private static List<EmpleadoDto> GetEmpleadosIniciales()
        {
            return new List<EmpleadoDto>
            {
                new EmpleadoDto { Id = 1, Nombre = "FRANCISCO", Apellido = "AGUIRRE", Email = "franciscoaguirre@gmail.com", Rol = "CAJERO", Telefono = "03493 456789", Estado = "ACTIVO" },
                new EmpleadoDto { Id = 2, Nombre = "ROCIO", Apellido = "MILANESE", Email = "rocimilanese@gmail.com", Rol = "CAJERA", Telefono = "03493 667173", Estado = "ACTIVO" }
            };
        }

        private static List<ProductoDto> GetProductosFallback()
        {
            return new List<ProductoDto>
            {
                new ProductoDto { Id = 1, Nombre = "BUZO VCV", Precio = 80000, Talles = "M/L", Color = "BEIGE", Stock = 10, Categoria = "abrigos", ImagenUrl = "/images/hombres.png", Disponible = true },
                new ProductoDto { Id = 2, Nombre = "REMERA ACTIVE", Precio = 20000, Talles = "XS/S/M", Color = "NEGRO", Stock = 14, Categoria = "remeras", ImagenUrl = "/images/mujeres.png", Disponible = true },
                new ProductoDto { Id = 3, Nombre = "HOODIE URBAN", Precio = 65000, Talles = "L/XL", Color = "VERDE", Stock = 8, Categoria = "abrigos", ImagenUrl = "/images/hombres2.png", Disponible = true }
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
                        new DetallePedidoDto { ProductoId = 2, ProductoNombre = "REMERA ACTIVE", Cantidad = 2, PrecioUnitario = 20000 }
                    }
                }
            };
        }

        private static List<ClienteDto> GetClientesIniciales()
        {
            return new List<ClienteDto>
            {
                new ClienteDto { Id = 1, Dni = "38456789", Nombre = "FRANCISCO", Apellido = "AGUIRRE", FechaAlta = new DateTime(2025, 07, 25), Email = "franciscoaguirre@gmail.com", Telefono = "03493 456789", Estado = "ACTIVO" },
                new ClienteDto { Id = 2, Dni = "39667173", Nombre = "ROCIO", Apellido = "MILANESE", FechaAlta = new DateTime(2025, 07, 29), Email = "rocimilanese@gmail.com", Telefono = "03493 667173", Estado = "ACTIVO" }
            };
        }
    }
}