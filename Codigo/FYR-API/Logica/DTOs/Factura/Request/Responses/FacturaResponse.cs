namespace DTO.Factura.Response;

public class FacturaResponse
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public int Numero { get; set; }

    public decimal Total { get; set; }

    public string FormaPago { get; set; } = string.Empty;

    public int PedidoId { get; set; }
}