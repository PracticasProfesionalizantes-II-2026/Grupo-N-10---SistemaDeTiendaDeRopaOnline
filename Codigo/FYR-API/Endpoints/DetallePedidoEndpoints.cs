public static class DetallePedidoEndpoints
{
    public static void MapDetallePedidoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pedidos/{pedidoId}/detalles")
        .WithTags("Detalles de Pedido")
        .WithGroupName("Detalles de Pedido");

        group.MapGet("/", async (int pedidoId, IDetallePedidoService service) =>
            Results.Ok(await service.GetByPedidoAsync(pedidoId)));

        group.MapGet("/{detalleId}", async (int pedidoId, int detalleId, IDetallePedidoService service) =>
        {
            var detalle = await service.GetByIdAsync(pedidoId, detalleId);
            return detalle is null ? Results.NotFound() : Results.Ok(detalle);
        });

        group.MapPost("/", async (int pedidoId, CreateDetallePedidoRequest request, IDetallePedidoService service) =>
        {
            var detalle = await service.CreateAsync(pedidoId, request);
            return Results.Created($"/pedidos/{pedidoId}/detalles/{detalle.IdPedidoDetalle}", detalle);
        });

        group.MapPut("/{detalleId}", async (int pedidoId, int detalleId, UpdateDetallePedidoRequest request, IDetallePedidoService service) =>
        {
            var detalle = await service.UpdateAsync(pedidoId, detalleId, request);
            return detalle is null ? Results.NotFound() : Results.Ok(detalle);
        });

        group.MapDelete("/{detalleId}", async (int pedidoId, int detalleId, IDetallePedidoService service) =>
        {
            var deleted = await service.DeleteAsync(pedidoId, detalleId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}