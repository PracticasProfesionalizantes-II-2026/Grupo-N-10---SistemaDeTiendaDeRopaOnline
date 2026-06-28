public static class EnvioEndpoints
{
    public static void MapEnvioEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pedidos/{pedidoId}/envios");

        group.MapGet("/", async (int pedidoId, IEnvioService service) =>
            Results.Ok(await service.GetByPedidoAsync(pedidoId)));

        group.MapGet("/{envioId}", async (int pedidoId, int envioId, IEnvioService service) =>
        {
            var envio = await service.GetByIdAsync(pedidoId, envioId);
            return envio is null ? Results.NotFound() : Results.Ok(envio);
        });

        group.MapPost("/", async (int pedidoId, CreateEnvioRequest request, IEnvioService service) =>
        {
            var envio = await service.CreateAsync(pedidoId, request);
            return Results.Created($"/pedidos/{pedidoId}/envios/{envio.IdEnvio}", envio);
        });

        group.MapPut("/{envioId}", async (int pedidoId, int envioId, UpdateEnvioRequest request, IEnvioService service) =>
        {
            var envio = await service.UpdateAsync(pedidoId, envioId, request);
            return envio is null ? Results.NotFound() : Results.Ok(envio);
        });

        group.MapPatch("/{envioId}/estado", async (int pedidoId, int envioId, UpdateEstadoEnvioRequest request, IEnvioService service) =>
        {
            var envio = await service.UpdateEstadoAsync(pedidoId, envioId, request);
            return envio is null ? Results.NotFound() : Results.Ok(envio);
        });

        group.MapDelete("/{envioId}", async (int pedidoId, int envioId, IEnvioService service) =>
        {
            var deleted = await service.DeleteAsync(pedidoId, envioId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        app.MapPost("/envios/calcular", async (CalcularEnvioRequest request, IEnvioService service) =>
            Results.Ok(await service.CalcularAsync(request)));

        app.MapGet("/envios/{envioId}", async (int envioId, IEnvioService service) =>
        {
            var envio = await service.GetTrackingAsync(envioId);
            return envio is null ? Results.NotFound() : Results.Ok(envio);
        });
    }
}