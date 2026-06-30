using DTO.MedioContacto.Request;
using Repositorios.Interfaces;

namespace Endpoints;

public static class MedioContactoEndpoints
{
    public static RouteGroupBuilder MapMedioContactoEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IMedioContactoRepository repo) =>
        {
            return Results.Ok(await repo.GetAllAsync());
        });

        group.MapGet("/{id:int}", async (int id, IMedioContactoRepository repo) =>
        {
            var medio = await repo.GetByIdAsync(id);

            return medio is null
                ? Results.NotFound()
                : Results.Ok(medio);
        });

        group.MapPost("/", async (CreateMedioContactoRequest request, IMedioContactoRepository repo) =>
        {
            var medio = await repo.CreateAsync(request);

            return Results.Created($"/api/medioscontacto/{medio.Id}", medio);
        });

        group.MapPut("/{id:int}", async (int id, UpdateMedioContactoRequest request, IMedioContactoRepository repo) =>
        {
            var actualizado = await repo.UpdateAsync(id, request);

            return actualizado
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, IMedioContactoRepository repo) =>
        {
            var eliminado = await repo.DeleteAsync(id);

            return eliminado
                ? Results.NoContent()
                : Results.NotFound();
        });

        return group;
    }
}