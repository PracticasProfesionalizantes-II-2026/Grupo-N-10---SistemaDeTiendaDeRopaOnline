using DTO.Producto.Requests;
using Repositorios.Interfaces;

namespace Endpoints;

public static class ProductoEndpoints
{
    public static void MapProductoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/productos")
            .WithTags("Productos");

        group.MapGet("/", async (IProductoRepository repository) =>
        {
            return Results.Ok(await repository.GetAllAsync());
        });

        group.MapGet("/{id:int}", async (int id, IProductoRepository repository) =>
        {
            var producto = await repository.GetByIdAsync(id);

            return producto is null
                ? Results.NotFound()
                : Results.Ok(producto);
        });

        group.MapPost("/", async (CreateProductoRequest request,
                                  IProductoRepository repository) =>
        {
            var producto = await repository.CreateAsync(request);

            return Results.Created($"/api/productos/{producto.Id}", producto);
        });

        group.MapPut("/{id:int}", async (int id,
                                         UpdateProductoRequest request,
                                         IProductoRepository repository) =>
        {
            var actualizado = await repository.UpdateAsync(id, request);

            return actualizado
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id,
                                            IProductoRepository repository) =>
        {
            var eliminado = await repository.DeleteAsync(id);

            return eliminado
                ? Results.NoContent()
                : Results.NotFound();
        });
    }
}