using Entidades.Enums;

public class PagoResponse
{
    public int IdPago { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; }
    public EstadoPago Estado { get; set; }
    public int PedidoId { get; set; }
}