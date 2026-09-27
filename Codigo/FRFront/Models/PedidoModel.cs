namespace FRFront.Models
{
    public class PedidoModel
    {
        public string NroPedido { get; set; } = "";
        public string Estado { get; set; } = "En Camino";
        public string DetalleFecha { get; set; } = "";
        public int TotalProductos { get; set; }
        public string ImagenProducto { get; set; } = "";
        public List<ItemCarrito> Productos { get; set; } = new List<ItemCarrito>();
    }
}