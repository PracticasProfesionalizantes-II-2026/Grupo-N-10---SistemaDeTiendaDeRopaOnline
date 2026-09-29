using System.ComponentModel.DataAnnotations;

namespace FRFront.Models;

public class DifusionNotificacionViewModel
{
    [Required(ErrorMessage = "El mensaje es obligatorio.")]
    [StringLength(1000, ErrorMessage = "El mensaje no puede superar los 1000 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    public IFormFile? Imagen { get; set; }
}
