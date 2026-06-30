public interface IReporteService
{
    Task<List<ReporteResponse>> GetByUsuarioAsync(int usuarioId);
    Task<ReporteResponse?> GetByIdAsync(int usuarioId, int reporteId);
    Task<ReporteResponse> CreateAsync(int usuarioId, CreateReporteRequest request);
    Task<List<ReporteResponse>> FilterByFechaAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<GenerateReporteResponse> GenerateAsync(GenerateReporteRequest request);
    Task<bool> DeleteAsync(int usuarioId, int reporteId);
}