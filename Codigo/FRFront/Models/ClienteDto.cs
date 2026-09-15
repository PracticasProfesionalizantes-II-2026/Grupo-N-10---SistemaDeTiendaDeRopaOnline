namespace FRFront.Models
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string NumeroCliente => $"#{Id:D9}";
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaAlta { get; set; }
        public string Estado { get; set; } = "ACTIVO";
    }
}