namespace DTO.Sucursal.Response;

public class SucursalResponse
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public int EmpresaId { get; set; }
}