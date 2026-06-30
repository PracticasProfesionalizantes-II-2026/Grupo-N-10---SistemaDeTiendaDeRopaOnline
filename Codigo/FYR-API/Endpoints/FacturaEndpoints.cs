using DTO.Factura.Request;
using Repositorios.Interfaces;

namespace Endpoints;

public static class FacturaEndpoints
{
    public static RouteGroupBuilder MapFacturaEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IFacturaRepository repo) =>
        {
            return Results.Ok(await repo.GetAllAsync());
        });

        group.MapGet("/{id:int}", async (int id, IFacturaRepository repo) =>
        {
            var factura = await repo.GetByIdAsync(id);

            return factura is null
                ? Results.NotFound()
                : Results.Ok(factura);
        });

        group.MapPost("/", async (CreateFacturaRequest request, IFacturaRepository repo) =>
        {
            var factura = await repo.CreateAsync(request);

            return Results.Created($"/api/facturas/{factura.Id}", factura);
        });

        group.MapPut("/{id:int}", async (int id, UpdateFacturaRequest request, IFacturaRepository repo) =>
        {
            var actualizado = await repo.UpdateAsync(id, request);

            return actualizado
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, IFacturaRepository repo) =>
        {
            var eliminado = await repo.DeleteAsync(id);

            return eliminado
                ? Results.NoContent()
                : Results.NotFound();
        });

        return group;
    }
}