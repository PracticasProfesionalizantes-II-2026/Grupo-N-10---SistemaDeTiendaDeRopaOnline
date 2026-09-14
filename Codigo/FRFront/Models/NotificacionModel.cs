namespace FRFront.Models
{
    public class NotificacionModel
    {
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Oferta"; // Puede ser "Envio", "Oferta", etc.
    }
}