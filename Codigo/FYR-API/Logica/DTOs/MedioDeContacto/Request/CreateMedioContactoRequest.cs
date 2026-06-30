using System.ComponentModel.DataAnnotations;

namespace DTO.MedioContacto.Request;

public class CreateMedioContactoRequest
{
    [Required]
    public string Medio { get; set; } = string.Empty;

    [Required]
    public string Valor { get; set; } = string.Empty;

    public int EmpresaId { get; set; }
}