using System.ComponentModel.DataAnnotations;

namespace Entidades.Models;

public class Sucursal
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Direccion { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    // FK Empresa
    public int EmpresaId { get; set; }

    public Empresa Empresa { get; set; } = null!;

    // Relaciones
    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
}