using System.ComponentModel.DataAnnotations;
namespace Entidades.Models;

public class DetallePedido
{
    [Key]
    public int Id { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public int ProductoId { get; set; }

    public Producto Producto { get; set; } = null!;

    public int PedidoId { get; set; }

    public Pedido Pedido { get; set; } = null!;
}