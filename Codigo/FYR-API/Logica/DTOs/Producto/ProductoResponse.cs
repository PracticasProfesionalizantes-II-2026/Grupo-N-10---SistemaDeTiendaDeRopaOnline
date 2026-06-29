namespace DTO.Producto.Responses;

public class ProductoResponse
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public string? ImagenUrl { get; set; }

    public string? Talles { get; set; }

    public string? Colores { get; set; }

    public string Categoria { get; set; } = string.Empty;

    public string? Subcategoria { get; set; }

    public string Empresa { get; set; } = string.Empty;
}