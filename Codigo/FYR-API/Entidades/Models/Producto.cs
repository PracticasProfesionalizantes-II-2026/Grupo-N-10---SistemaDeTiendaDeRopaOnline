using System.ComponentModel.DataAnnotations;

namespace Entidades.Models;

public class Producto
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public string? ImagenUrl { get; set; }

    public string? Talles { get; set; }

    public string? Colores { get; set; }

    // FK Empresa
    public int EmpresaId { get; set; }

    public Empresa Empresa { get; set; } = null!;

    // FK Categoria
    public int CategoriaId { get; set; }

    public Categoria Categoria { get; set; } = null!;

    // FK Subcategoria (Opcional)
    public int? SubcategoriaId { get; set; }

    public Subcategoria? Subcategoria { get; set; }

    // Relaciones
    public ICollection<DetallePedido> DetallesPedido { get; set; } = new List<DetallePedido>();

    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();

    public ICollection<ProductoProveedor> ProductosProveedor { get; set; } = new List<ProductoProveedor>();
    public bool Activo { get; set; } = true;
}