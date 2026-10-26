namespace DTO.Producto.Requests;

public class BuscarProductoRequest
{
    public string? Texto { get; set; }
    public int? CategoriaId { get; set; }
    public int? SubcategoriaId { get; set; }
    public string? Genero { get; set; }
    public string? Color { get; set; }
    public string? Talle { get; set; }
    public decimal? PrecioMinimo { get; set; }
    public decimal? PrecioMaximo { get; set; }
    public bool? SoloOfertas { get; set; }
    public bool? SoloDisponibles { get; set; }
    public bool IncluirInactivos { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 20;
}
