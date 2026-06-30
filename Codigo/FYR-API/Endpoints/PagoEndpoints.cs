using DTO.Pago.Request;
using Repositorios.Interfaces;

namespace Endpoints;

public static class PagoEndpoints
{
    public static RouteGroupBuilder MapPagoEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IPagoRepository repo) =>
        {
            return Results.Ok(await repo.GetAllAsync());
        });

        group.MapGet("/{id:int}", async (int id, IPagoRepository repo) =>
        {
            var pago = await repo.GetByIdAsync(id);

            return pago is null
                ? Results.NotFound()
                : Results.Ok(pago);
        });

        group.MapPost("/", async (CreatePagoRequest request, IPagoRepository repo) =>
        {
            var pago = await repo.CreateAsync(request);

            return Results.Created($"/api/pagos/{pago.Id}", pago);
        });

        group.MapPut("/{id:int}", async (int id, UpdatePagoRequest request, IPagoRepository repo) =>
        {
            return await repo.UpdateAsync(id, request)
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, IPagoRepository repo) =>
        {
            return await repo.DeleteAsync(id)
                ? Results.NoContent()
                : Results.NotFound();
        });

        return group;
    }
}