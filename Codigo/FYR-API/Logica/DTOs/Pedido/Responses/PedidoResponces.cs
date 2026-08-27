public class PedidoResponse
{
    public int IdPedido { get; set; }
    public DateTime FechaPedido { get; set; }
    public decimal Total { get; set; }
    public string DireccionEntrega { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public string? NumeroSeguimiento { get; set; }
    public int UsuarioId { get; set; }
    public string Estado { get; set; } = string.Empty;
}