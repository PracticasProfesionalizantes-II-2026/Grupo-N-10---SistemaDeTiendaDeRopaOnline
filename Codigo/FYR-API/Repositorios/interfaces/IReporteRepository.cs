using Entidades.Models;

public interface IReporteRepository
{
    Task<List<Reporte>> GetByUsuarioIdAsync(int usuarioId);
    Task<Reporte?> GetByIdAsync(int usuarioId, int reporteId);
    Task<List<Reporte>> FilterByFechaAsync(DateTime fechaInicio, DateTime fechaFin);
    Task AddAsync(Reporte reporte);
    Task DeleteAsync(Reporte reporte);
}