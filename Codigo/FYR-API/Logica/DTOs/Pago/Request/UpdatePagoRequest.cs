using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace DTO.Pago.Request;

public class UpdatePagoRequest
{
    [Required]
    public decimal Monto { get; set; }

    [Required]
    public string MetodoPago { get; set; } = string.Empty;

    [Required]
    public EstadoPago Estado { get; set; }

    [Required]
    public int PedidoId { get; set; }
}