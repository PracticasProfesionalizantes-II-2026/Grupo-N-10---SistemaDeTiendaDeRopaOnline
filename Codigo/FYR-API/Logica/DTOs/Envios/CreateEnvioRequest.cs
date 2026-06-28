
public class CreateEnvioRequest
{
    public string TipoEnvio { get; set; } = string.Empty;
    public decimal Costo { get; set; }
    public DateTime FechaEstimada { get; set; }
    public string? NumeroSeguimiento { get; set; }
    public string Estado { get; set; } = string.Empty;
}