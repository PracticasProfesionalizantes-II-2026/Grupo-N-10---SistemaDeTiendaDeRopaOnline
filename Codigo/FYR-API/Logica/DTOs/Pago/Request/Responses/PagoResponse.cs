using Entidades.Enums;

namespace DTO.Pago.Response;

public class PagoResponse
{
    public int Id { get; set; }

    public decimal Monto { get; set; }

    public string MetodoPago { get; set; } = string.Empty;

    public EstadoPago Estado { get; set; }

    public DateTime FechaPago { get; set; }

    public int PedidoId { get; set; }
}