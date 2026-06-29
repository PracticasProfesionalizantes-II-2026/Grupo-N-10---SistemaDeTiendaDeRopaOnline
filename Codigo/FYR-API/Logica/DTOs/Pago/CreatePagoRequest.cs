using Entidades.Enums;

public class CreatePagoRequest
{
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; }
    public EstadoPago Estado { get; set; }
}