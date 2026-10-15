namespace DTO.Proveedor.Response;

public class ProveedorResponse
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string? Correo { get; set; }
    public string? Direccion { get; set; }

    public int EmpresaId { get; set; }
}