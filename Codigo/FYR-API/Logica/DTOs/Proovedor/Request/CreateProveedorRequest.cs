using System.ComponentModel.DataAnnotations;

namespace DTO.Proveedor.Request;

public class CreateProveedorRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    [EmailAddress]
    public string? Correo { get; set; }
    public string? Direccion { get; set; }

    [Required]
    public int EmpresaId { get; set; }
}