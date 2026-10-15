namespace DTO.Proveedor.Request;

public class ProveedorCompraResponse
{
    public int ProductoProveedorId { get; set; }
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public decimal PrecioCompra { get; set; }
    public int Cantidad { get; set; }
}
