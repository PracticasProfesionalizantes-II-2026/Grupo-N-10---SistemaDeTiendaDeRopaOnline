//crear con la estructura de subcategoriaendpoints
using Microsoft.AspNetCore.Mvc;

namespace Endpoints;
public static class ClienteEndpoints
{
    public static void MapClienteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/clientes")
            .WithTags("Clientes");

        group.MapGet("/", async ([FromServices] IClienteService service) =>
        {
            var clientes = await service.GetAllAsync();
            return Results.Ok(clientes);
        });

        group.MapGet("/{id:int}", async (int id, [FromServices] IClienteService service) =>
        {
            var cliente = await service.GetByIdAsync(id);
            if (cliente == null)
                return Results.NotFound();
            return Results.Ok(cliente);
        });

        group.MapPost("/", async ([FromBody] CreateClienteRequest request, [FromServices] IClienteService service) =>
        {
            var cliente = await service.CreateAsync(request);
            var idProperty = cliente.GetType().GetProperty("Id")
                ?? cliente.GetType().GetProperty("ClienteId")
                ?? cliente.GetType().GetProperty("IdCliente");
            var location = idProperty != null
                ? $"/api/clientes/{idProperty.GetValue(cliente)}"
                : "/api/clientes";
            return Results.Created(location, cliente);
        });

        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateClienteRequest request, [FromServices] IClienteService service) =>
        {
            var updatedCliente = await service.UpdateAsync(id, request);
            if (updatedCliente == null)
                return Results.NotFound();
            return Results.Ok(updatedCliente);
        });

        group.MapDelete("/{id:int}", async (int id, [FromServices] IClienteService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            if (!deleted)
                return Results.NotFound();
            return Results.NoContent();
        });
    }
}   