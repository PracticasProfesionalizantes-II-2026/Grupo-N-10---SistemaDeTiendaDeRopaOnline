using DTO.Sucursal.Request;
using Repositorios.Interfaces;

namespace Endpoints;

public static class SucursalEndpoints
{
    public static RouteGroupBuilder MapSucursalEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISucursalRepository repo) =>
        {
            return Results.Ok(await repo.GetAllAsync());
        });

        group.MapGet("/{id:int}", async (int id, ISucursalRepository repo) =>
        {
            var sucursal = await repo.GetByIdAsync(id);

            return sucursal is null
                ? Results.NotFound()
                : Results.Ok(sucursal);
        });

        group.MapPost("/", async (CreateSucursalRequest request, ISucursalRepository repo) =>
        {
            var sucursal = await repo.CreateAsync(request);

            return Results.Created($"/api/sucursales/{sucursal.Id}", sucursal);
        });

        group.MapPut("/{id:int}", async (int id, UpdateSucursalRequest request, ISucursalRepository repo) =>
        {
            var actualizado = await repo.UpdateAsync(id, request);

            return actualizado
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, ISucursalRepository repo) =>
        {
            var eliminado = await repo.DeleteAsync(id);

            return eliminado
                ? Results.NoContent()
                : Results.NotFound();
        });

        return group;
    }
}