using System.ComponentModel.DataAnnotations;
using Entidades.Enums;
namespace Entidades.Models;

public class Pago
{
    [Key]
    public int Id { get; set; }

    public decimal Monto { get; set; }

    public string MetodoPago { get; set; } = string.Empty;

    public DateTime FechaPago { get; set; } = DateTime.UtcNow;

    public EstadoPago Estado { get; set; }  

    public int PedidoId { get; set; }

    public Pedido Pedido { get; set; } = null!;
}