namespace DTO.Producto.Responses;

public class BuscarProductoResponse
{
    public IReadOnlyList<ProductoResponse> Productos { get; init; } = [];
    public int Total { get; init; }
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; }
    public int TotalPaginas { get; init; }
}
