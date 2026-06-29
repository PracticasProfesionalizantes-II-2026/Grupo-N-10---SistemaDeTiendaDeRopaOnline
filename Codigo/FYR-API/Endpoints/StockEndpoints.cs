using DTO.Stock.Requests;
using Repositorios.Interfaces;

namespace Endpoints;

public static class StockEndpoints
{
    public static void MapStockEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/stocks")
            .WithTags("Stocks");

        group.MapGet("/", async (IStockRepository repository) =>
        {
            return Results.Ok(await repository.GetAllAsync());
        });

        group.MapGet("/{id:int}", async (int id, IStockRepository repository) =>
        {
            var stock = await repository.GetByIdAsync(id);

            return stock is null
                ? Results.NotFound()
                : Results.Ok(stock);
        });

        group.MapPost("/", async (CreateStockRequest request, IStockRepository repository) =>
        {
            var stock = await repository.CreateAsync(request);

            return Results.Created($"/stocks/{stock.Id}", stock);
        });

        group.MapPut("/{id:int}", async (int id, UpdateStockRequest request, IStockRepository repository) =>
        {
            var actualizado = await repository.UpdateAsync(id, request);

            return actualizado
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, IStockRepository repository) =>
        {
            var eliminado = await repository.DeleteAsync(id);

            return eliminado
                ? Results.NoContent()
                : Results.NotFound();
        });
    }
}