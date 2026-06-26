using System.ComponentModel.DataAnnotations;

namespace Entidades.Models;

public class Notificacion
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Mensaje { get; set; } = string.Empty;
    public ICollection<Notificacion> Notificaciones { get; set; }
    = new List<Notificacion>();

    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;

    public bool Leida { get; set; } = false;

    // FK Usuario
    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;
}