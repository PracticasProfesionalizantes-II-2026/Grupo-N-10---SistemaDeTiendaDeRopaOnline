using DTO.IA;
using Logica.Interfaces;

namespace Endpoints;

public static class IaCatalogoEndpoints
{
    public static void MapIaCatalogoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/ia/catalogo")
            .WithTags("IA - Catálogo");

        group.MapPost("/buscar", async (
            CatalogoIaRequest request,
            IGeminiCatalogoService service,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Consulta))
                return Results.BadRequest(new { mensaje = "La consulta no puede estar vacía." });

            if (request.Consulta.Length > 500)
                return Results.BadRequest(new { mensaje = "La consulta no puede superar los 500 caracteres." });

            return Results.Ok(await service.BuscarAsync(request, cancellationToken));
        })
        .WithSummary("Interpreta una búsqueda de ropa en lenguaje natural con Gemini.");

        group.MapPost("/outfit", async (
            OutfitIaRequest request,
            IGeminiCatalogoService service,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Preferencias))
                return Results.BadRequest(new { mensaje = "Las preferencias no pueden estar vacías." });

            if (request.Preferencias.Length > 500)
                return Results.BadRequest(new { mensaje = "Las preferencias no pueden superar los 500 caracteres." });

            return Results.Ok(await service.RecomendarOutfitAsync(request, cancellationToken));
        })
        .WithSummary("Recomienda un outfit usando productos disponibles del catálogo.");
    }
}
