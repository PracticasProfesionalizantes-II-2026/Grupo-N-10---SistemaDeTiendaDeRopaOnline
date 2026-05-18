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
    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();

    public string? ImagenUrl { get; set; }

    public string? Talles { get; set; }

    public string? Colores { get; set; }

    public int EmpresaId { get; set; }

    public Empresa Empresa { get; set; } = null!;

    public int CategoriaId { get; set; }

    public Categoria Categoria { get; set; } = null!;

    public ICollection<DetallePedido> DetallesPedido { get; set; } = new List<DetallePedido>();
}