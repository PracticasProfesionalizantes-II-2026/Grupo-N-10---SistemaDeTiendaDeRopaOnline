public static class ReporteEndpoints
{
    public static void MapReporteEndpoints(this WebApplication app)
    {
        var userGroup = app.MapGroup("/api/usuarios/{usuarioId}/reportes")
            .WithTags("Reportes");

        var globalGroup = app.MapGroup("/api/reportes")
            .WithTags("Reportes");

        userGroup.MapGet("/", async (int usuarioId, IReporteService service) =>
            Results.Ok(await service.GetByUsuarioAsync(usuarioId)));

        userGroup.MapGet("/{reporteId}", async (int usuarioId, int reporteId, IReporteService service) =>
        {
            var reporte = await service.GetByIdAsync(usuarioId, reporteId);
            return reporte is null ? Results.NotFound() : Results.Ok(reporte);
        });

        userGroup.MapPost("/", async (int usuarioId, CreateReporteRequest request, IReporteService service) =>
        {
            var reporte = await service.CreateAsync(usuarioId, request);
            return Results.Created($"/api/usuarios/{usuarioId}/reportes/{reporte.IdReporte}", reporte);
        });

        userGroup.MapDelete("/{reporteId}", async (int usuarioId, int reporteId, IReporteService service) =>
        {
            var deleted = await service.DeleteAsync(usuarioId, reporteId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        globalGroup.MapPost("/generar", async (GenerateReporteRequest request, IReporteService service) =>
            Results.Ok(await service.GenerateAsync(request)));

        globalGroup.MapGet("/", async (
            DateTime fechaInicio,
            DateTime fechaFin,
            IReporteService service) =>
        {
            return Results.Ok(await service.FilterByFechaAsync(fechaInicio, fechaFin));
        });
    }
}