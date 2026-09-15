namespace FRFront.Models
{
    public class Producto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal? PrecioAnterior { get; set; } // Opcional para mostrar tachado si está en oferta
        public bool EsOferta { get; set; }           // True si va a la sección Ofertas
        public string Genero { get; set; }        = string.Empty;   // "Hombre" o "Mujer"
        public string Categoria { get; set; }   = string.Empty;     // "Abrigos", "Pantalones", "Remeras", etc.
        public string Descripcion { get; set; }= string.Empty;
        public string Imagen { get; set; }= string.Empty;
        public string[] Talles { get; set; } = Array.Empty<string>();
        public bool SinStock { get; set; }
    }
}