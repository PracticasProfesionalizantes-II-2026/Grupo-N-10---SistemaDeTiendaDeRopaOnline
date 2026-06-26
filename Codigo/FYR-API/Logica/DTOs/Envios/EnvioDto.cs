public class EnvioDto
{
    public int IdEnvio { get; set; }
    public string TipoEnvio { get; set; } = null!;
    public decimal Costo { get; set; }
    public DateTime FechaEstimada { get; set; }
    public string NumeroSeguimiento { get; set; } = null!;
    public int IdEstado { get; set; }
}