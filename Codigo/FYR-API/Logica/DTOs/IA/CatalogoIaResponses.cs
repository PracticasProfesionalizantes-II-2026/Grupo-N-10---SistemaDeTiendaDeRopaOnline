using DTO.Producto.Responses;

namespace DTO.IA;

public sealed record CatalogoIaResponse(
    string ConsultaInterpretada,
    IReadOnlyList<ProductoResponse> Productos,
    string Mensaje);

public sealed record OutfitIaResponse(
    string Explicacion,
    IReadOnlyList<ProductoResponse> Prendas);
