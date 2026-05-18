using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades.Models;

public class Pedido
{
    [Key]
    public int Id { get; set; }

    public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

    public decimal Total { get; set; }

    public string DireccionEntrega { get; set; } = string.Empty;

    public EstadoPedido Estado { get; set; }

    public string MetodoPago { get; set; } = string.Empty;

    public string? NumeroSeguimiento { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public ICollection<DetallePedido> DetallesPedido { get; set; } = new List<DetallePedido>();

    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}