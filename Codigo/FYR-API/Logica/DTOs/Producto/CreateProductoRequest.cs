namespace DTO.Producto.Requests;

public class CreateProductoRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Marca { get; set; }

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public decimal? PrecioAnterior { get; set; }

    public bool EsOferta { get; set; }

    public string? ImagenUrl { get; set; }

    public string? Talles { get; set; }

    public string? Colores { get; set; }

    public string? Genero { get; set; }

    public int EmpresaId { get; set; }

    public int CategoriaId { get; set; }

    public int? SubcategoriaId { get; set; }

    public int Stock { get; set; }
    public int? ProveedorId { get; set; }
    public decimal? PrecioCompra { get; set; }
}