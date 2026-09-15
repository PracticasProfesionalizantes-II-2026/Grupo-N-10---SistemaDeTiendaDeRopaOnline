using System;

namespace FRFront.Models
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string NumeroCliente => $"#{Id:D10}";
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
        public DateTime FechaAlta { get; set; } = DateTime.Now;
        public string Email { get; set; } = string.Empty;
        public string Estado { get; set; } = "ACTIVO"; // ACTIVO, INACTIVO, BLOQUEADO
    }
}