
public class ReporteResponse
{
    public int IdReporte { get; set; }
    public string TipoReporte { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? Datos { get; set; }
    public int UsuarioId { get; set; }
}