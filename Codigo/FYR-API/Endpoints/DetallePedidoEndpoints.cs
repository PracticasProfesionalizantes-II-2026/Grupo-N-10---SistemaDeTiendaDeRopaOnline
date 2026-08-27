using Logica.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Endpoints;

public static class DetallePedidoEndpoints
{
    public static void MapDetallePedidoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/pedidos/{pedidoId:int}/detalles")
            .WithTags("DetallePedido");

        // Obtener todos los detalles de un pedido
        group.MapGet("/", async (
            int pedidoId,
            [FromServices] IDetallePedidoService service) =>
        {
            var detalles = await service.GetByPedidoAsync(pedidoId);
            return Results.Ok(detalles);
        });

        // Obtener un detalle por Id
        group.MapGet("/{detalleId:int}", async (
            int pedidoId,
            int detalleId,
            [FromServices] IDetallePedidoService service) =>
        {
            var detalle = await service.GetByIdAsync(pedidoId, detalleId);

            if (detalle == null)
                return Results.NotFound();

            return Results.Ok(detalle);
        });

        // Crear un detalle
        group.MapPost("/", async (
            int pedidoId,
            [FromBody] CreateDetallePedidoRequest request,
            [FromServices] IDetallePedidoService service) =>
        {
            var detalle = await service.CreateAsync(pedidoId, request);

            return Results.Created(
                $"/api/pedidos/{pedidoId}/detalles/{detalle.IdPedidoDetalle}",
                detalle);
        });

        // Modificar un detalle
        group.MapPut("/{detalleId:int}", async (
            int pedidoId,
            int detalleId,
            [FromBody] UpdateDetallePedidoRequest request,
            [FromServices] IDetallePedidoService service) =>
        {
            var detalle = await service.UpdateAsync(pedidoId, detalleId, request);

            if (detalle == null)
                return Results.NotFound();

            return Results.Ok(detalle);
        });

        // Eliminar un detalle
        group.MapDelete("/{detalleId:int}", async (
            int pedidoId,
            int detalleId,
            [FromServices] IDetallePedidoService service) =>
        {
            var eliminado = await service.DeleteAsync(pedidoId, detalleId);

            if (!eliminado)
                return Results.NotFound();

            return Results.NoContent();
        });
    }
}