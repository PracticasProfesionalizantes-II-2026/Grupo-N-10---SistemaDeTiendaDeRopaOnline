using System.ComponentModel.DataAnnotations;

namespace Logica.DTO.Subcategoria.Requests;

public class CreateSubcategoriaRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public int CategoriaId { get; set; }
}