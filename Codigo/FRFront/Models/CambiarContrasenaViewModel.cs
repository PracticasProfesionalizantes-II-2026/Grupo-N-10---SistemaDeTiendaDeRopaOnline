using System.ComponentModel.DataAnnotations;

namespace FRFront.Models
{
    public class CambiarContrasenaViewModel
    {
        [Required(ErrorMessage = "Debes ingresar tu contraseña actual.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual")]
        public string ContrasenaActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes ingresar la nueva contraseña.")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NuevaContrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes confirmar la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Compare("NuevaContrasena", ErrorMessage = "La nueva contraseña y su confirmación no coinciden.")]
        [Display(Name = "Confirmar Nueva Contraseña")]
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}