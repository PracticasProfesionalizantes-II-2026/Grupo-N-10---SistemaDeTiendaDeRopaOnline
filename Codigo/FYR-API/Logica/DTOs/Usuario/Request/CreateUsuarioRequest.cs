using Entidades.Enums;
using System.ComponentModel.DataAnnotations;

namespace DTO.Usuario.Request;

public class CreateUsuarioRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public Rol Rol { get; set; }

    public string? Telefono { get; set; }

    public string? IdiomaPreferido { get; set; }

    public string? FotoPerfil { get; set; }

    public int? EmpresaId { get; set; }
}