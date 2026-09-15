namespace FRFront.Models
{
    public class ReporteVentasDto
    {
        public DateTime FechaDesde { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime FechaHasta { get; set; } = DateTime.Today;
        public decimal TotalVentas { get; set; }
        public int TotalPedidos { get; set; }
        public decimal TicketPromedio => TotalPedidos > 0 ? TotalVentas / TotalPedidos : 0;
        public List<VentaDetalleReporte> ListadoVentas { get; set; } = new List<VentaDetalleReporte>();
        
        // Propiedades agrupadas para el gráfico
        public List<string> FechasGrafico { get; set; } = new List<string>();
        public List<decimal> TotalesGrafico { get; set; } = new List<decimal>();
    }

    public class VentaDetalleReporte
    {
        public int PedidoId { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string TipoEntrega { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}