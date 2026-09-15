using System;

namespace FRFront.Models
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Dni { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaAlta { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "ACTIVO";

        public string NumeroCliente => $"CLI-{Id:D3}";
        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    }
}