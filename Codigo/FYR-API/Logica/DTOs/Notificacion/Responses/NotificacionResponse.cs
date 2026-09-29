public class NotificacionResponse
{
    public int IdNotificacion { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public DateTime FechaEnvio { get; set; }
    public bool Leida { get; set; }
    public int UsuarioId { get; set; }
}