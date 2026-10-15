using System.ComponentModel.DataAnnotations;

namespace FRFront.Models;

public class ProveedorAdminModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    public string? Correo { get; set; }
    public string? Direccion { get; set; }

    public int EmpresaId { get; set; } = 1;

    public string WhatsAppUrl
    {
        get
        {
            var telefono = new string((Telefono ?? string.Empty).Where(char.IsDigit).ToArray());
            return string.IsNullOrWhiteSpace(telefono)
                ? string.Empty
                : $"https://wa.me/{telefono}?text={Uri.EscapeDataString($"Hola {Nombre}, queremos realizar un pedido.")}";
        }
    }
}

public class ProveedorCompraAdminModel
{
    public int ProductoProveedorId { get; set; }
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public decimal PrecioCompra { get; set; }
    public int Cantidad { get; set; }
}

public class ProveedorGestionViewModel
{
    public ProveedorAdminModel Formulario { get; set; } = new();
    public List<ProveedorAdminModel> Proveedores { get; set; } = new();
}

public class ProveedorComprasViewModel
{
    public ProveedorAdminModel Proveedor { get; set; } = new();
    public List<ProveedorCompraAdminModel> Compras { get; set; } = new();
}
