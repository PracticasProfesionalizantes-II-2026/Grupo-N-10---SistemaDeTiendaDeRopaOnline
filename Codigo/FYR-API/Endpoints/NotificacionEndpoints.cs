using Datos;
using Entidades.Enums;
using Entidades.Models;
using Microsoft.EntityFrameworkCore;

public static class NotificacionEndpoints
{
    public static RouteGroupBuilder MapNotificacionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notificaciones")
            .WithTags("Notificaciones");

        group.MapGet("/", async (INotificacionService service) =>
            Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id:int}", async (int id, INotificacionService service) =>
        {
            var notificacion = await service.GetByIdAsync(id);
            return notificacion is null ? Results.NotFound() : Results.Ok(notificacion);
        });

        group.MapGet("/usuario/{usuarioId:int}", async (int usuarioId, AppDbContext db) =>
        {
            var notificaciones = await db.Notificaciones
                .AsNoTracking()
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaEnvio)
                .Select(n => new NotificacionResponse
                {
                    IdNotificacion = n.Id,
                    Mensaje = n.Mensaje,
                    ImagenUrl = n.ImagenUrl,
                    FechaEnvio = n.FechaEnvio,
                    Leida = n.Leida,
                    UsuarioId = n.UsuarioId
                })
                .ToListAsync();

            return Results.Ok(notificaciones);
        });

        group.MapPost("/", async (CreateNotificacionRequest request, INotificacionService service) =>
        {
            var notificacion = await service.CreateAsync(request);
            return Results.Created($"/api/notificaciones/{notificacion.IdNotificacion}", notificacion);
        });

        group.MapPost("/difusion", async (CreateDifusionRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Mensaje))
            {
                return Results.BadRequest("El mensaje es obligatorio.");
            }

            var clientes = await db.Usuarios
                .Where(u => u.Rol == Rol.Cliente && u.Activo)
                .Select(u => u.Id)
                .ToListAsync();

            var notificaciones = clientes.Select(usuarioId => new Notificacion
            {
                UsuarioId = usuarioId,
                Mensaje = request.Mensaje.Trim(),
                ImagenUrl = request.ImagenUrl,
                FechaEnvio = DateTime.UtcNow
            }).ToList();

            db.Notificaciones.AddRange(notificaciones);
            await db.SaveChangesAsync();

            return Results.Ok(new { enviados = notificaciones.Count });
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