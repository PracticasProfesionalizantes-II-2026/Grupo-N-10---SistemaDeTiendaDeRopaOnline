
namespace FRFront.Models
{
    public class PedidoModel
    {
        public string NroPedido { get; set; } = "#00000";
        public string Estado { get; set; } = "En Camino"; // "Entregado" o "En Camino"
        public string DetalleFecha { get; set; } = string.Empty;
        public int TotalProductos { get; set; } = 1;
        public string ImagenProducto { get; set; } = "~/images/buzovcv.png";
    }

}