using System.ComponentModel.DataAnnotations;

namespace Logica.DTO.Subcategoria.Requests;

public class UpdateSubcategoriaRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public int CategoriaId { get; set; }
}