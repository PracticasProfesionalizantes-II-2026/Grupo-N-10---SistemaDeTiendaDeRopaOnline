using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades.Models;

public class Envio
{
    [Key]
    public int IdEnvio { get; set; }

    public TipoEnvio TipoEnvio { get; set; }

    public decimal Costo { get; set; }

    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;

    public string? NumeroSeguimiento { get; set; }

    // NUEVO
    public EstadoEnvio Estado { get; set; }

    // FK Pedido
    public int PedidoId { get; set; }

    public Pedido Pedido { get; set; } = null!;
}