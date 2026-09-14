namespace FRFront.Models
{
    public class ItemCarrito
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string Imagen { get; set; } = string.Empty;
        public decimal Total => Precio * Cantidad;
    }
}