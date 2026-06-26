namespace Logica.DTO.Subcategoria.Responses;

public class SubcategoriaResponse
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int CategoriaId { get; set; }
}