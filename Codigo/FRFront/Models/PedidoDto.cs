using System;
using System.Collections.Generic;

namespace FRFront.Models
{
    public class PedidoDto
    {
        public int Id { get; set; }
        public string NumeroPedido => $"#{Id:D5}";
        public string Cliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public string Estado { get; set; } = "CONFIRMADO"; // CONFIRMADO, EN CAMINO, ENTREGADO, CANCELADO
        public string TipoEntrega { get; set; } = "RETIRO LOCAL"; // RETIRO LOCAL, ENVÍO A DOMICILIO
        public List<DetallePedidoDto> Detalle { get; set; } = new List<DetallePedidoDto>();
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