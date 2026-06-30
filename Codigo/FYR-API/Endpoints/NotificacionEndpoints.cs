public static class NotificacionEndpoints
{
    public static RouteGroupBuilder MapNotificacionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/notificaciones")
        .WithTags("Notificaciones")
        .WithGroupName("Notificaciones");

        group.MapGet("/", async (INotificacionService service) =>
            Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id}", async (int id, INotificacionService service) =>
        {
            var notificacion = await service.GetByIdAsync(id);
            return notificacion is null ? Results.NotFound() : Results.Ok(notificacion);
        });

        group.MapPost("/", async (CreateNotificacionRequest request, INotificacionService service) =>
        {
            var notificacion = await service.CreateAsync(request);
            return Results.Created($"/notificaciones/{notificacion.IdNotificacion}", notificacion);
        });

        group.MapPatch("/{id}/estado", async (int id, UpdateEstadoNotificacionRequest request, INotificacionService service) =>
        {
            var notificacion = await service.UpdateEstadoAsync(id, request);
            return notificacion is null ? Results.NotFound() : Results.Ok(notificacion);
        });

        group.MapDelete("/{id}", async (int id, INotificacionService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
         return group;
    }
}