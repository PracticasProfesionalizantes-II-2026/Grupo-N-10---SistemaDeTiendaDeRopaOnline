using System.ComponentModel.DataAnnotations;
namespace Entidades.Models;

public class Categoria
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    public int EmpresaId { get; set; }

    public Empresa Empresa { get; set; } = null!;

    public ICollection<Subcategoria> Subcategorias { get; set; } = new List<Subcategoria>();

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}