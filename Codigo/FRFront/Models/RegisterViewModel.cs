using System.ComponentModel.DataAnnotations;

namespace FRFront.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [StringLength(20, ErrorMessage = "El DNI no puede superar los 20 caracteres.")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El domicilio es obligatorio.")]
        [StringLength(200, ErrorMessage = "El domicilio no puede superar los 200 caracteres.")]
        public string Domicilio { get; set; } = string.Empty;

        [Required(ErrorMessage = "La localidad es obligatoria.")]
        [StringLength(100, ErrorMessage = "La localidad no puede superar los 100 caracteres.")]
        public string Localidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}