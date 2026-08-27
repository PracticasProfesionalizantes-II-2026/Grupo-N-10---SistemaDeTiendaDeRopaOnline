// Models/ForgotPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace FRFront.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Ingresá tu correo electrónico")]
        [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido")]
        public string Email { get; set; } = string.Empty;
    }
}