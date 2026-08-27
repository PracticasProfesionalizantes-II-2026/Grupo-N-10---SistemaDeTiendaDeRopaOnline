public class EnvioResponse
{
    public int IdEnvio { get; set; }
    public string TipoEnvio { get; set; } = string.Empty;
    public decimal Costo { get; set; }
    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    public string? NumeroSeguimiento { get; set; }
    public string Estado { get; set; } = string.Empty;
    
}