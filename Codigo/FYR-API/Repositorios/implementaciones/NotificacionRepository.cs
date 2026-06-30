using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Datos;
public class NotificacionRepository : INotificacionRepository
{
    private readonly AppDbContext _context;

    public NotificacionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Notificacion>> GetAllAsync()
    {
        return await _context.Notificaciones
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Notificacion?> GetByIdAsync(int id)
    {
        return await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task AddAsync(Notificacion notificacion)
    {
        await _context.Notificaciones.AddAsync(notificacion);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Notificacion notificacion)
    {
        _context.Notificaciones.Update(notificacion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Notificacion notificacion)
    {
        _context.Notificaciones.Remove(notificacion);
        await _context.SaveChangesAsync();
    }
}