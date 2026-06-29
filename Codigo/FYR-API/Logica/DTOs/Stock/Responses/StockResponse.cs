namespace DTO.Stock.Responses;

public class StockResponse
{
    public int Id { get; set; }

    public int CantidadDisponible { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public string Estado { get; set; } = string.Empty;

    public int ProductoId { get; set; }

    public string Producto { get; set; } = string.Empty;

    public int SucursalId { get; set; }

    public string Sucursal { get; set; } = string.Empty;
}