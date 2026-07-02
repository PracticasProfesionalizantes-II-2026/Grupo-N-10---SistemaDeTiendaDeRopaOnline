namespace DTO.Producto.Requests;

public class UpdateProductoRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public string? ImagenUrl { get; set; }

    public string? Talles { get; set; }

    public string? Colores { get; set; }

    public int CategoriaId { get; set; }

    public int? SubcategoriaId { get; set; }
}