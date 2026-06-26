namespace Logica.DTOs.Envios;

public class ActualizarEnvioDto
{
    public string TipoEnvio { get; set; } = null!;
    public decimal Costo { get; set; }
    public DateTime FechaEstimada { get; set; }
    public int IdEstado { get; set; }
    public string NumeroSeguimiento { get; set; } = null!;
}