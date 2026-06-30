public static class PedidoEndpoints
{
    public static RouteGroupBuilder MapPedidoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pedidos")
        .WithTags("Pedidos")
        .WithGroupName("Pedidos");

        group.MapGet("/", async (IPedidoService service) =>
            Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id}", async (int id, IPedidoService service) =>
        {
            var pedido = await service.GetByIdAsync(id);
            return pedido is null ? Results.NotFound() : Results.Ok(pedido);
        });

        group.MapPost("/", async (CreatePedidoRequest request, IPedidoService service) =>
        {
            var pedido = await service.CreateAsync(request);
            return Results.Created($"/pedidos/{pedido.IdPedido}", pedido);
        });

        group.MapPut("/{id}", async (int id, UpdatePedidoRequest request, IPedidoService service) =>
        {
            var pedido = await service.UpdateAsync(id, request);
            return pedido is null ? Results.NotFound() : Results.Ok(pedido);
        });

        group.MapPatch("/{id}/estado", async (int id, UpdateEstadoPedidoRequest request, IPedidoService service) =>
        {
            var pedido = await service.UpdateEstadoAsync(id, request);
            return pedido is null ? Results.NotFound() : Results.Ok(pedido);
        });

        group.MapPatch("/{id}/seguimiento", async (int id, UpdateSeguimientoPedidoRequest request, IPedidoService service) =>
        {
            var pedido = await service.UpdateSeguimientoAsync(id, request);
            return pedido is null ? Results.NotFound() : Results.Ok(pedido);
        });

        group.MapDelete("/{id}", async (int id, IPedidoService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        app.MapGet("/usuarios/{usuarioId}/pedidos", async (int usuarioId, IPedidoService service) =>
            Results.Ok(await service.GetByUsuarioAsync(usuarioId)));
        return group;
    }
}