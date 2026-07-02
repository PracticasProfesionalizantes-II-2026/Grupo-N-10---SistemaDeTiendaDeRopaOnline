using System.ComponentModel.DataAnnotations;

namespace Logica.DTO.Empresa.Request;

public class ActualizarEmpresaRequest
{
    [Required]
    public string NombreComercial { get; set; } = string.Empty;

    [Required]
    public string LogoUrl { get; set; } = string.Empty;

    [Required]
    public string ColorPrimario { get; set; } = string.Empty;

    public string? ColorSecundario { get; set; }

    [Required]
    public string Tipografia { get; set; } = string.Empty;

    [Required]
    public string Cuit { get; set; } = string.Empty;

    [Required]
    public string RazonSocial { get; set; } = string.Empty;

    [Required]
    public string CondicionIVA { get; set; } = string.Empty;

    [Required]
    public string Direccion { get; set; } = string.Empty;

    public bool Activa { get; set; }
}