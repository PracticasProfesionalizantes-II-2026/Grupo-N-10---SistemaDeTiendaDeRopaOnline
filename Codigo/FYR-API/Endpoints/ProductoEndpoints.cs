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

        group.MapGet("/buscar", async (BuscarProductoRequest request,
                                        IProductoRepository repository) =>
        {
            if (request.Pagina < 1)
                return Results.BadRequest(new { mensaje = "La página debe ser mayor o igual a 1." });

            if (request.TamanoPagina is < 1 or > 100)
                return Results.BadRequest(new { mensaje = "El tamaño de página debe estar entre 1 y 100." });

            if (request.PrecioMinimo < 0 || request.PrecioMaximo < 0)
                return Results.BadRequest(new { mensaje = "Los precios no pueden ser negativos." });

            if (request.PrecioMinimo > request.PrecioMaximo)
                return Results.BadRequest(new { mensaje = "El precio mínimo no puede superar al precio máximo." });

            return Results.Ok(await repository.SearchAsync(request));
        })
        .WithName("BuscarProductos")
        .WithSummary("Busca productos del catálogo con filtros opcionales.");

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