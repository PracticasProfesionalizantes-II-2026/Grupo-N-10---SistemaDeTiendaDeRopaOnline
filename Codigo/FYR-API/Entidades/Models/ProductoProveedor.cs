using System.ComponentModel.DataAnnotations;
namespace Entidades.Models;

public class ProductoProveedor
{
    [Key]
    public int Id { get; set; }

    public decimal PrecioCompra { get; set; }

    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    // FK Producto
    public int ProductoId { get; set; }

    public Producto Producto { get; set; } = null!;

    // FK Proveedor
    public int ProveedorId { get; set; }

    public Proveedor Proveedor { get; set; } = null!;
}