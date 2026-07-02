using System.ComponentModel.DataAnnotations;

namespace DTO.Factura.Request;

public class UpdateFacturaRequest
{
    public DateTime Fecha { get; set; }

    [Required]
    public string Tipo { get; set; } = string.Empty;

    public int Numero { get; set; }

    public decimal Total { get; set; }

    [Required]
    public string FormaPago { get; set; } = string.Empty;

    public int PedidoId { get; set; }
}