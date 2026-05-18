using System.ComponentModel.DataAnnotations;
namespace Entidades.Models;

public class Reporte
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string TipoReporte { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public string? Datos { get; set; }

    // FK Usuario
    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;
}