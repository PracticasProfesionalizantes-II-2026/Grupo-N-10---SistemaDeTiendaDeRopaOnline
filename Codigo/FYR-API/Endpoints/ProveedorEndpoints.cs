using DTO.Proveedor.Request;
using Repositorios.Interfaces;

namespace Endpoints;

public static class ProveedorEndpoints
{
    public static void MapProveedorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/proveedores")
            .WithTags("Proveedores");

        group.MapGet("/", async (IProveedorRepository repo) =>
        {
            return Results.Ok(await repo.GetAllAsync());
        });

        group.MapGet("/{id:int}", async (int id, IProveedorRepository repo) =>
        {
            var proveedor = await repo.GetByIdAsync(id);

            return proveedor is null
                ? Results.NotFound()
                : Results.Ok(proveedor);
        });

        group.MapPost("/", async (CreateProveedorRequest request, IProveedorRepository repo) =>
        {
            var proveedor = await repo.CreateAsync(request);

            return Results.Created($"/api/proveedores/{proveedor.Id}", proveedor);
        });

        group.MapPut("/{id:int}", async (int id, UpdateProveedorRequest request, IProveedorRepository repo) =>
        {
            var ok = await repo.UpdateAsync(id, request);

            return ok
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, IProveedorRepository repo) =>
        {
            var ok = await repo.DeleteAsync(id);

            return ok
                ? Results.NoContent()
                : Results.NotFound();
        });
    }
}