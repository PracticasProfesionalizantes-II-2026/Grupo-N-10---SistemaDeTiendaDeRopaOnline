namespace DTO.Producto.Responses;

public class ProductoResponse
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public decimal? PrecioAnterior { get; set; }

    public bool EsOferta { get; set; }

    public string? ImagenUrl { get; set; }

    public string? Talles { get; set; }

    public string? Colores { get; set; }

    public string Categoria { get; set; } = string.Empty;

    public int EmpresaId { get; set; }

    public int CategoriaId { get; set; }

    public int? SubcategoriaId { get; set; }

    public string? Subcategoria { get; set; }

    public string Empresa { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public int Stock { get; set; }
}