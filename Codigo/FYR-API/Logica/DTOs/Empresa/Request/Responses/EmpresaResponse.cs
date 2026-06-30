namespace Logica.DTO.Empresa.Response;

public class EmpresaResponse
{
    public int Id { get; set; }

    public string NombreComercial { get; set; } = string.Empty;

    public string LogoUrl { get; set; } = string.Empty;

    public string ColorPrimario { get; set; } = string.Empty;

    public string? ColorSecundario { get; set; }

    public string Tipografia { get; set; } = string.Empty;

    public string Cuit { get; set; } = string.Empty;

    public string RazonSocial { get; set; } = string.Empty;

    public string CondicionIVA { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public bool Activa { get; set; }
}