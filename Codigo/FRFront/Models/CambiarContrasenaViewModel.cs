using System.ComponentModel.DataAnnotations;

namespace FRFront.Models
{
    public class CambiarContrasenaViewModel
    {
        [Required(ErrorMessage = "Debes ingresar tu correo electrónico.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        // Se mantienen para el flujo independiente de cambio de contraseña del administrador.
        [DataType(DataType.Password)]
        public string ContrasenaActual { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string NuevaContrasena { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}