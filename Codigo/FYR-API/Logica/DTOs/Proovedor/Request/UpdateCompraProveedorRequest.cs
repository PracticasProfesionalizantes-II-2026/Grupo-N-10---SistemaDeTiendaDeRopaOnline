using System.ComponentModel.DataAnnotations;

namespace DTO.Proveedor.Request;

public class UpdateCompraProveedorRequest
{
    [Range(0, int.MaxValue)]
    public int Cantidad { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PrecioCompra { get; set; }
}
