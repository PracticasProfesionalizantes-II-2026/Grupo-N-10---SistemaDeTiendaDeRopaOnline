using Entidades.Enums;

namespace DTO.Stock.Requests;

public class UpdateStockRequest
{
    public int CantidadDisponible { get; set; }

    public EstadoStock Estado { get; set; }

    public int SucursalId { get; set; }
}