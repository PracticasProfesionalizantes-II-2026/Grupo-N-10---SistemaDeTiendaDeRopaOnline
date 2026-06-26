public class TrackingEnvioDto
{
    public int IdEnvio { get; set; }
    public int IdEstado { get; set; }
    public string Estado { get; set; } = null!;
    public DateTime FechaEstimada { get; set; }
    public string NumeroSeguimiento { get; set; } = null!;
}