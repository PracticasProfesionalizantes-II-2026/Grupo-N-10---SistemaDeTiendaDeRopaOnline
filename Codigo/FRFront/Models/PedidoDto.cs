using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FRFront.Models
{
    public class PedidoDto
    {
        public int Id { get; set; }
        public int IdPedido { get; set; }
        public int UsuarioId { get; set; }
        public string NumeroPedido => $"#{(Id > 0 ? Id : IdPedido):D5}";
        public string Cliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        [JsonConverter(typeof(EstadoPedidoJsonConverter))]
        public string Estado { get; set; } = "CONFIRMADO"; // CONFIRMADO, EN CAMINO, ENTREGADO, CANCELADO
        public string TipoEntrega { get; set; } = "RETIRO LOCAL"; // RETIRO LOCAL, ENVÍO A DOMICILIO
        public List<DetallePedidoDto> Detalle { get; set; } = new List<DetallePedidoDto>();
        public DateTime FechaPedido { get; set; }
        public string DireccionEntrega { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public string? NumeroSeguimiento { get; set; }
    }

    public class DetallePedidoDto
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}