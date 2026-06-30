using Logica.DTO.Subcategoria.Requests;
using Logica.Interfaces;

namespace Endpoints;

public static class SubcategoriaEndpoints
{
    public static void MapSubcategoriaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/subcategorias")
            .WithTags("Subcategorias");

        group.MapGet("/", async (ISubcategoriaService service) =>
        {
            return Results.Ok(await service.GetAllAsync());
        });

        group.MapGet("/{id:int}", async (int id, ISubcategoriaService service) =>
        {
            var subcategoria = await service.GetByIdAsync(id);

            return subcategoria is null
                ? Results.NotFound()
                : Results.Ok(subcategoria);
        });

        group.MapPost("/", async (CreateSubcategoriaRequest request, ISubcategoriaService service) =>
        {
            var subcategoria = await service.CreateAsync(request);

            return Results.Created($"/api/subcategorias/{subcategoria.Id}", subcategoria);
        });

        group.MapPut("/{id:int}", async (int id, UpdateSubcategoriaRequest request, ISubcategoriaService service) =>
        {
            var ok = await service.UpdateAsync(id, request);

            return ok
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, ISubcategoriaService service) =>
        {
            var ok = await service.DeleteAsync(id);

            return ok
                ? Results.NoContent()
                : Results.NotFound();
        });
    }
}