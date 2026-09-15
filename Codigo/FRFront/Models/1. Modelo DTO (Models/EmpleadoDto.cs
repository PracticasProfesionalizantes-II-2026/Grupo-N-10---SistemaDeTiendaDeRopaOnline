using System.ComponentModel.DataAnnotations;

namespace FRFront.Models
{
    public class EmpleadoDto
    {
        public int Id { get; set; }

        public string NumeroEmpleado => $"#{Id:D4}";

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; } = string.Empty;

        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; } = "CAJERO";

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        public string Telefono { get; set; } = string.Empty;

        public string Estado { get; set; } = "ACTIVO";
    }
}