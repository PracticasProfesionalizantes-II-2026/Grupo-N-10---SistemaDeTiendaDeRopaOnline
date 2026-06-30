using Logica.DTO.Categoria.Requests;
using Logica.Interfaces;

namespace Endpoints;

public static class CategoriaEndpoints
{
    public static void MapCategoriaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categorias")
            .WithTags("Categorias")
            .WithGroupName("Categorias");

        group.MapGet("/", async (ICategoriaService service) =>
        {
            return Results.Ok(await service.GetAllAsync());
        });

        group.MapGet("/{id:int}", async (int id, ICategoriaService service) =>
        {
            var categoria = await service.GetByIdAsync(id);

            return categoria is null
                ? Results.NotFound()
                : Results.Ok(categoria);
        });

        group.MapPost("/", async (CreateCategoriaRequest request, ICategoriaService service) =>
        {
            var categoria = await service.CreateAsync(request);

            return Results.Created($"/api/categorias/{categoria.Id}", categoria);
        });

        group.MapPut("/{id:int}", async (int id, UpdateCategoriaRequest request, ICategoriaService service) =>
        {
            var ok = await service.UpdateAsync(id, request);

            return ok
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, ICategoriaService service) =>
        {
            var ok = await service.DeleteAsync(id);

            return ok
                ? Results.NoContent()
                : Results.NotFound();
        });
    }
}