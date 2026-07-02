using System.ComponentModel.DataAnnotations;

namespace Logica.DTO.Categoria.Requests;

public class CreateCategoriaRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public int EmpresaId { get; set; }
}