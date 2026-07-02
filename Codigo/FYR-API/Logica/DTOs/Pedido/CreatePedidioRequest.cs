public class CreatePedidoRequest
{
    public string DireccionEntrega { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string Estado { get; set; } = string.Empty;
}