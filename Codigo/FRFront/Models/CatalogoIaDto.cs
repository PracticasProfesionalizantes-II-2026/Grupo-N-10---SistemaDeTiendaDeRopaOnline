namespace FRFront.Models;

public sealed class CatalogoIaDto
{
    public string ConsultaInterpretada { get; set; } = string.Empty;
    public List<ProductoDto> Productos { get; set; } = [];
    public string Mensaje { get; set; } = string.Empty;
}

public sealed class OutfitIaDto
{
    public string Explicacion { get; set; } = string.Empty;
    public List<ProductoDto> Prendas { get; set; } = [];
}
