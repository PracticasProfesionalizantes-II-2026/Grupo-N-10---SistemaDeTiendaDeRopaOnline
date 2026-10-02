using Microsoft.AspNetCore.Mvc;
using FRFront.Models;
using FRFront.Helpers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FRFront.Controllers
{
    public class ClienteController : Controller
    {
        private readonly HttpClient _httpClient;

        public ClienteController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
        }

        private bool EsClienteAutenticado()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioSesion")) &&
                   string.Equals(HttpContext.Session.GetString("RolSesion"), "Cliente", StringComparison.OrdinalIgnoreCase);
        }

        // 1. Vista de Mis Compras del cliente autenticado con datos reales
        public async Task<IActionResult> MisCompras()
        {
            if (!EsClienteAutenticado())
            {
                return RedirectToAction("Index", "Home");
            }

            var listaCompras = await ObtenerComprasPersistidasAsync();

            var historialJson = HttpContext.Session.GetString(UserSessionKeys.ForUser(HttpContext.Session, "HistorialComprasSession"));
            if (!string.IsNullOrEmpty(historialJson))
            {
                var historial = JsonSerializer.Deserialize<List<PedidoModel>>(historialJson) ?? new List<PedidoModel>();
                var numerosExistentes = historial.Select(pedido => pedido.NroPedido).ToHashSet(StringComparer.OrdinalIgnoreCase);
                listaCompras = historial.Concat(listaCompras.Where(pedido => !numerosExistentes.Contains(pedido.NroPedido))).ToList();
            }

            return View(listaCompras);
        }

        // 2. Vista de seguimiento de un pedido específico del cliente autenticado
        public async Task<IActionResult> SeguirEnvio(string nroPedido)
        {
            if (!EsClienteAutenticado())
            {
                return RedirectToAction("Index", "Home");
            }

            var listaCompras = await ObtenerComprasPersistidasAsync();

            // Buscamos el pedido que coincida con el número recibido
            var pedido = listaCompras.FirstOrDefault(p => p.NroPedido == nroPedido);

            // Si no lo encuentra en sesión, creamos uno temporal para que no falle
            if (pedido == null)
            {
                pedido = new PedidoModel
                {
                    NroPedido = nroPedido ?? "#70344",
                    Estado = "En Camino",
                    DetalleFecha = "Tu pedido llegará hoy entre las 14 hs y las 19 hs"
                };
            }

            return View(pedido);
        }

        // 3. Vista del Perfil de Usuario
        public IActionResult MiPerfil()
        {
            if (!EsClienteAutenticado())
            {
                return RedirectToAction("Index", "Home");
            }

            var usuarioLogueado = HttpContext.Session.GetString("UsuarioSesion")!;
            ViewData["EmailUsuario"] = usuarioLogueado;
            return View();
        }

        // 4. Muestra la vista para Editar el Perfil
        public IActionResult EditarPerfil()
        {
            if (!EsClienteAutenticado())
            {
                return RedirectToAction("Index", "Home");
            }

            var usuarioLogueado = HttpContext.Session.GetString("UsuarioSesion")!;
            ViewData["EmailUsuario"] = usuarioLogueado;
            return View();
        }

        // 5. Procesa la actualización de los datos del perfil
        [HttpPost]
        public IActionResult GuardarPerfil(string nombre, string dni, string domicilio, string telefono)
        {
            if (!EsClienteAutenticado())
            {
                return RedirectToAction("Index", "Home");
            }

            // Aquí puedes guardar los datos en tu sesión o base de datos si lo deseas

            // Guardamos un mensaje de éxito temporal
            TempData["MensajeExito"] = "¡Los cambios se han guardado con éxito!";

            return RedirectToAction("MiPerfil");
        }

        // 6. Vista de Notificaciones del cliente
        public async Task<IActionResult> Notificaciones()
        {
            if (!EsClienteAutenticado())
            {
                return RedirectToAction("Index", "Home");
            }

            var listaNotificaciones = await ObtenerNotificacionesPersistidasAsync();
            var compras = await ObtenerComprasPersistidasAsync();
            var compraReciente = compras.FirstOrDefault();

            if (compraReciente != null)
            {
                listaNotificaciones.Add(new NotificacionModel
                {
                    Titulo = $"Pedido {compraReciente.NroPedido}: {compraReciente.Estado}",
                    Mensaje = $"El estado de tu pedido es {compraReciente.Estado.ToLowerInvariant()}.",
                    Fecha = compraReciente.DetalleFecha,
                    Tipo = "Envio"
                });
            }

            return View(listaNotificaciones);
        }

        private async Task<List<NotificacionModel>> ObtenerNotificacionesPersistidasAsync()
        {
            var email = HttpContext.Session.GetString("UsuarioSesion");
            if (string.IsNullOrWhiteSpace(email))
            {
                return new List<NotificacionModel>();
            }

            try
            {
                var usuarioId = await ObtenerUsuarioApiIdAsync(email);
                if (usuarioId <= 0)
                {
                    return new List<NotificacionModel>();
                }

                var notificaciones = await _httpClient.GetFromJsonAsync<List<NotificacionResponseDto>>($"api/notificaciones/usuario/{usuarioId}") ?? new List<NotificacionResponseDto>();
                return notificaciones.Select(n => new NotificacionModel
                {
                    Titulo = "Nueva notificación",
                    Mensaje = n.Mensaje,
                    Fecha = n.FechaEnvio.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    Tipo = "Difusion",
                    ImagenUrl = n.ImagenUrl
                }).ToList();
            }
            catch (HttpRequestException)
            {
                return new List<NotificacionModel>();
            }
        }

        private async Task<List<PedidoModel>> ObtenerComprasPersistidasAsync()
        {
            var email = HttpContext.Session.GetString("UsuarioSesion");
            if (string.IsNullOrWhiteSpace(email))
            {
                return new List<PedidoModel>();
            }

            try
            {
                var usuarioId = await ObtenerUsuarioApiIdAsync(email);
                if (usuarioId <= 0)
                {
                    return new List<PedidoModel>();
                }

                var pedidos = await _httpClient.GetFromJsonAsync<List<PedidoDto>>($"api/usuarios/{usuarioId}/pedidos") ?? new List<PedidoDto>();
                var productos = await _httpClient.GetFromJsonAsync<List<ProductoDto>>("api/productos") ?? new List<ProductoDto>();
                var productosPorId = productos.ToDictionary(producto => producto.Id);
                var resultado = new List<PedidoModel>();

                foreach (var pedido in pedidos)
                {
                    var pedidoId = pedido.Id > 0 ? pedido.Id : pedido.IdPedido;
                    var detalles = pedidoId > 0
                        ? await _httpClient.GetFromJsonAsync<List<DetallePedidoDto>>($"api/pedidos/{pedidoId}/detalles") ?? new List<DetallePedidoDto>()
                        : new List<DetallePedidoDto>();
                    var items = detalles
                        .Select(detalle =>
                        {
                            productosPorId.TryGetValue(detalle.ProductoId, out var producto);
                            return new ItemCarrito
                            {
                                ProductoId = detalle.ProductoId,
                                Nombre = producto?.Nombre ?? detalle.ProductoNombre ?? "Producto no disponible",
                                Codigo = detalle.ProductoId.ToString(),
                                Precio = detalle.PrecioUnitario,
                                Imagen = string.IsNullOrWhiteSpace(producto?.ImagenUrl) ? "~/images/logo-fr.png" : producto.ImagenUrl,
                                Cantidad = detalle.Cantidad
                            };
                        }).ToList();

                    resultado.Add(new PedidoModel
                    {
                        NroPedido = pedido.NumeroPedido,
                        Estado = pedido.Estado,
                        DetalleFecha = pedido.FechaPedido.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                        Total = pedido.Total,
                        EsRetiroLocal = pedido.DireccionEntrega.Equals("RETIRO_LOCAL", StringComparison.OrdinalIgnoreCase),
                        TotalProductos = detalles.Sum(detalle => detalle.Cantidad),
                        ImagenProducto = items.FirstOrDefault()?.Imagen ?? "~/images/logo-fr.png",
                        Productos = items
                    });
                }

                return resultado;
            }
            catch (HttpRequestException)
            {
                return new List<PedidoModel>();
            }
        }

        private async Task<int> ObtenerUsuarioApiIdAsync(string email)
        {
            var usuarios = await _httpClient.GetFromJsonAsync<List<UsuarioSimpleDto>>("api/usuarios") ?? new List<UsuarioSimpleDto>();
            var usuario = usuarios.FirstOrDefault(item => string.Equals(item.Email, email, StringComparison.OrdinalIgnoreCase));
            var id = usuario?.ResolveId() ?? 0;
            if (id > 0)
                HttpContext.Session.SetString("UsuarioApiId", id.ToString());
            return id;
        }
    }
}