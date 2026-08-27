using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Datos;

public class EnvioRepository : IEnvioRepository
{
    private readonly AppDbContext _context;

    public EnvioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Envio>> GetByPedidoIdAsync(int pedidoId)
    {
        return await _context.Envios
            .AsNoTracking()
            .Where(e => e.PedidoId == pedidoId)
            .ToListAsync();
    }

    public async Task<Envio?> GetByIdAsync(int pedidoId, int envioId)
    {
        return await _context.Envios
            .FirstOrDefaultAsync(e => e.PedidoId == pedidoId && e.IdEnvio == envioId);
    }

    public async Task<Envio?> GetTrackingAsync(int envioId)
    {
        return await _context.Envios
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdEnvio == envioId);
    }

    public async Task AddAsync(Envio envio)
    {
        await _context.Envios.AddAsync(envio);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Envio envio)
    {
        _context.Envios.Update(envio);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Envio envio)
    {
        _context.Envios.Remove(envio);
        await _context.SaveChangesAsync();
    }
}