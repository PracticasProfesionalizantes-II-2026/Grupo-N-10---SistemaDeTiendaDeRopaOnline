using System.ComponentModel.DataAnnotations;

namespace FRFront.Models
{
    public class CambiarContrasenaViewModel
    {
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "NUEVA CONTRASEÑA")]
        public string NuevaPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "CONFIRMAR")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}