using System;

namespace FRFront.Models
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string Dni { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaAlta { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "ACTIVO";

        public string NumeroCliente => $"CLI-{Id:D3}";
        public string NombreCompleto
        {
            get
            {
                var nombre = Nombre.Trim();
                var apellido = Apellido.Trim();

                if (!string.IsNullOrWhiteSpace(nombre) &&
                    nombre.Equals(apellido, StringComparison.OrdinalIgnoreCase))
                {
                    return nombre;
                }

                var partesApellido = Apellido
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (partesApellido.Length > 1 &&
                    int.TryParse(partesApellido[^1], out _))
                {
                    return $"{nombre} {string.Join(' ', partesApellido[..^1])}".Trim();
                }

                return $"{nombre} {apellido}".Trim();
            }
        }
    }
}