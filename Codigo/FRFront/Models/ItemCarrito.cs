namespace FRFront.Models
{
    public class ItemCarrito
    {
        public string Nombre { get; set; } = "";
        public string Codigo { get; set; } = "";
        public decimal Precio { get; set; }
        public string Imagen { get; set; } = "";
        public string Talle { get; set; } = ""; // <-- Asegúrate de tener esta propiedad
        public int Cantidad { get; set; } = 1;
        public decimal Total => Precio * Cantidad;
    }
}