using System.ComponentModel.DataAnnotations;

namespace FRFront.Models;

public class ClientePerfilViewModel
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El domicilio es obligatorio.")]
    public string Domicilio { get; set; } = string.Empty;

    [Required(ErrorMessage = "La localidad es obligatoria.")]
    public string Localidad { get; set; } = string.Empty;

    public bool Activo { get; set; }
}
