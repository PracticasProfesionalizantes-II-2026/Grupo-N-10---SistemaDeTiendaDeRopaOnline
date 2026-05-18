using System.ComponentModel.DataAnnotations;
namespace Entidades.Models;

public class Proveedor
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    [EmailAddress]
    public string? Correo { get; set; }

    public ICollection<ProductoProveedor> ProductosProveedor { get; set; }
        = new List<ProductoProveedor>();
}