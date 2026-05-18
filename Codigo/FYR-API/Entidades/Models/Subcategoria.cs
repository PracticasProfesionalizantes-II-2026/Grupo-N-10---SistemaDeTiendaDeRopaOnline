using System.ComponentModel.DataAnnotations;
namespace Entidades.Models;

public class Subcategoria
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    // FK Categoria
    public int CategoriaId { get; set; }

    public Categoria Categoria { get; set; } = null!;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}