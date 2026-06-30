public class CreateReporteRequest
{
    public string TipoReporte { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}