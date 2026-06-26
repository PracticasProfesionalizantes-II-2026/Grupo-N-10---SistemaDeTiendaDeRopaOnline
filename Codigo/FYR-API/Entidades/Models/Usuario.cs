using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades.Models;

public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public Rol Rol { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public string? Telefono { get; set; }

    public string? IdiomaPreferido { get; set; }

    public bool Activo { get; set; } = true;

    // FK Empresa
    public int? EmpresaId { get; set; }

    public Empresa? Empresa { get; set; }

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();
    public ICollection<Notificacion> Notificaciones { get; set; }
    = new List<Notificacion>();
    public string? FotoPerfil { get; set; }
}
