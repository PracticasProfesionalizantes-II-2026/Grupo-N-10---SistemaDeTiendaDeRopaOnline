using Entidades.Enums;

public class UpdatePagoRequest
{
    public string MetodoPago { get; set; } = string.Empty;
    public EstadoPago Estado { get; set; }
}