using Entidades.Models;

public class ReporteService : IReporteService
{
    private readonly IReporteRepository _repository;

    public ReporteService(IReporteRepository repository)
    {
        _repository = repository;
    }

    private ReporteResponse Map(Reporte reporte)
    {
        return new ReporteResponse
        {
            IdReporte = reporte.Id,
            TipoReporte = reporte.TipoReporte,
            FechaInicio = reporte.FechaInicio,
            FechaFin = reporte.FechaFin,
            Datos = reporte.Datos,
            UsuarioId = reporte.UsuarioId
        };
    }

    public async Task<List<ReporteResponse>> GetByUsuarioAsync(int usuarioId)
    {
        var reportes = await _repository.GetByUsuarioIdAsync(usuarioId);
        return reportes.Select(Map).ToList();
    }

    public async Task<ReporteResponse?> GetByIdAsync(int usuarioId, int reporteId)
    {
        var reporte = await _repository.GetByIdAsync(usuarioId, reporteId);

        return reporte == null ? null : Map(reporte);
    }

    public async Task<ReporteResponse> CreateAsync(int usuarioId, CreateReporteRequest request)
    {
        var reporte = new Reporte
        {
            TipoReporte = request.TipoReporte,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            UsuarioId = usuarioId
        };

        await _repository.AddAsync(reporte);

        return Map(reporte);
    }

    public async Task<List<ReporteResponse>> FilterByFechaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var reportes = await _repository.FilterByFechaAsync(fechaInicio, fechaFin);
        return reportes.Select(Map).ToList();
    }

    public Task<GenerateReporteResponse> GenerateAsync(GenerateReporteRequest request)
    {
        var reporte = new GenerateReporteResponse
        {
            TipoReporte = request.TipoReporte,
            TotalVentas = 500000,
            CantidadPedidos = 120
        };

        return Task.FromResult(reporte);
    }

    public async Task<bool> DeleteAsync(int usuarioId, int reporteId)
    {
        var reporte = await _repository.GetByIdAsync(usuarioId, reporteId);

        if (reporte == null) return false;

        await _repository.DeleteAsync(reporte);

        return true;
    }
}