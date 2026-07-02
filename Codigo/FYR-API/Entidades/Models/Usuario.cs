using System.ComponentModel.DataAnnotations;

namespace Entidades.Models;

public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    // Acá se guarda el hash, nunca la contraseña en texto plano
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public Rol Rol { get; set; } = Rol.Cliente;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    [Phone]
    [MaxLength(30)]
    public string? Telefono { get; set; }

    [MaxLength(50)]
    public string? IdiomaPreferido { get; set; }

    public string? FotoPerfil { get; set; }

    public bool Activo { get; set; } = true;

    // Empresa a la que pertenece.
    // Los clientes no tienen empresa.
    public int? EmpresaId { get; set; }

    public Empresa? Empresa { get; set; }

    // Relaciones

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();

    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
}