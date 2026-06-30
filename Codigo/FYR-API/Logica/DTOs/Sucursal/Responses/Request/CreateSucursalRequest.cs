using System.ComponentModel.DataAnnotations;

namespace DTO.Sucursal.Request;

public class CreateSucursalRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Direccion { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public int EmpresaId { get; set; }
}