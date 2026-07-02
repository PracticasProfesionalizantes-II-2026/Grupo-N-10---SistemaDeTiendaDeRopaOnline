using System.ComponentModel.DataAnnotations;

namespace DTO.Proveedor.Request;

public class UpdateProveedorRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    [EmailAddress]
    public string? Correo { get; set; }

    [Required]
    public int EmpresaId { get; set; }
}