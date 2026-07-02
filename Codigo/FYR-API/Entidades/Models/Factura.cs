using System.ComponentModel.DataAnnotations;

namespace Entidades.Models;

public class Factura
{
    [Key]
    public int Id { get; set; }

    public DateTime Fecha { get; set; }
    public DateTime FechaFactura { get; set; } = DateTime.UtcNow;

    public string Tipo { get; set; } = string.Empty;

    public int Numero { get; set; }

    public decimal Total { get; set; }

    public string FormaPago { get; set; } = string.Empty;

    // FK Pedido
    public int PedidoId { get; set; }

    public Pedido Pedido { get; set; } = null!;
}