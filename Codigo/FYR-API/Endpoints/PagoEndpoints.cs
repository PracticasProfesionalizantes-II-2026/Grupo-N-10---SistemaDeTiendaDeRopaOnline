public static class PagoEndpoints
{
    public static void MapPagoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pedidos/{pedidoId}/pagos");

        group.MapGet("/", async (int pedidoId, IPagoService service) =>
            Results.Ok(await service.GetByPedidoAsync(pedidoId)));

        group.MapGet("/{pagoId}", async (int pedidoId, int pagoId, IPagoService service) =>
        {
            var pago = await service.GetByIdAsync(pedidoId, pagoId);
            return pago is null ? Results.NotFound() : Results.Ok(pago);
        });

        group.MapPost("/", async (int pedidoId, CreatePagoRequest request, IPagoService service) =>
        {
            var pago = await service.CreateAsync(pedidoId, request);
            return Results.Created($"/pedidos/{pedidoId}/pagos/{pago.IdPago}", pago);
        });

        group.MapPut("/{pagoId}", async (int pedidoId, int pagoId, UpdatePagoRequest request, IPagoService service) =>
        {
            var pago = await service.UpdateAsync(pedidoId, pagoId, request);
            return pago is null ? Results.NotFound() : Results.Ok(pago);
        });

        group.MapDelete("/{pagoId}", async (int pedidoId, int pagoId, IPagoService service) =>
        {
            var deleted = await service.DeleteAsync(pedidoId, pagoId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}