using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Datos;

public class ReporteRepository : IReporteRepository
{
    private readonly AppDbContext _context;

    public ReporteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Reporte>> GetByUsuarioIdAsync(int usuarioId)
    {
        return await _context.Reportes
            .AsNoTracking()
            .Where(r => r.UsuarioId == usuarioId)
            .ToListAsync();
    }

    public async Task<Reporte?> GetByIdAsync(int usuarioId, int reporteId)
    {
        return await _context.Reportes
            .FirstOrDefaultAsync(r => r.UsuarioId == usuarioId && r.Id == reporteId);
    }

    public async Task<List<Reporte>> FilterByFechaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _context.Reportes
            .AsNoTracking()
            .Where(r => r.FechaInicio >= fechaInicio && r.FechaFin <= fechaFin)
            .ToListAsync();
    }

    public async Task AddAsync(Reporte reporte)
    {
        await _context.Reportes.AddAsync(reporte);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Reporte reporte)
    {
        _context.Reportes.Remove(reporte);
        await _context.SaveChangesAsync();
    }
}