//crear con la estructura de subcategoriaendpoints
using Logica.DTO.Categoria.Requests;
using Logica.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace Endpoints;

public static class CategoriaEndpoints
{
    public static RouteGroupBuilder MapCategoriaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categorias")
            .WithTags("Categorias");

        group.MapGet("/", async ([FromServices] ICategoriaService service) =>
        {
            var categorias = await service.GetAllAsync();
            return Results.Ok(categorias);
        });

        group.MapGet("/{id:int}", async (int id, [FromServices] ICategoriaService service) =>
        {
            var categoria = await service.GetByIdAsync(id);
            if (categoria == null)
                return Results.NotFound();
            return Results.Ok(categoria);
        });

        group.MapPost("/", async ([FromBody] CreateCategoriaRequest request, [FromServices] ICategoriaService service) =>
        {
            var categoria = await service.CreateAsync(request);
            return Results.Created($"/api/categorias/{categoria.Id}", categoria);
        });

        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateCategoriaRequest request, [FromServices] ICategoriaService service) =>
        {
            var updated = await service.UpdateAsync(id, request);
            if (!updated)
                return Results.NotFound();
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, [FromServices] ICategoriaService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            if (!deleted)
                return Results.NotFound();
            return Results.NoContent();
        });

        return group;
    }
}