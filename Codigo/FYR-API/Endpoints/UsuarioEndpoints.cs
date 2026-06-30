using DTO.Usuario.Request;
using Repositorios.Interfaces;

namespace Endpoints;

public static class UsuarioEndpoints
{
    public static RouteGroupBuilder MapUsuarioEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IUsuarioRepository repo) =>
        {
            return Results.Ok(await repo.GetAllAsync());
        });

        group.MapGet("/{id:int}", async (int id, IUsuarioRepository repo) =>
        {
            var usuario = await repo.GetByIdAsync(id);

            return usuario is null
                ? Results.NotFound()
                : Results.Ok(usuario);
        });

        group.MapPost("/", async (CreateUsuarioRequest request, IUsuarioRepository repo) =>
        {
            var usuario = await repo.CreateAsync(request);

            return Results.Created($"/api/usuarios/{usuario.Id}", usuario);
        });

        group.MapPut("/{id:int}", async (int id, UpdateUsuarioRequest request, IUsuarioRepository repo) =>
        {
            return await repo.UpdateAsync(id, request)
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, IUsuarioRepository repo) =>
        {
            return await repo.DeleteAsync(id)
                ? Results.NoContent()
                : Results.NotFound();
        });

        return group;
    }
}