using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Datos;
public class PagoRepository : IPagoRepository
{
    private readonly AppDbContext _context;

    public PagoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Pago>> GetByPedidoIdAsync(int pedidoId)
    {
        return await _context.Pagos
            .AsNoTracking()
            .Where(p => p.PedidoId == pedidoId)
            .ToListAsync();
    }

    public async Task<Pago?> GetByIdAsync(int pedidoId, int pagoId)
    {
        return await _context.Pagos
            .FirstOrDefaultAsync(p => p.PedidoId == pedidoId && p.Id == pagoId);
    }

    public async Task AddAsync(Pago pago)
    {
        await _context.Pagos.AddAsync(pago);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Pago pago)
    {
        _context.Pagos.Update(pago);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Pago pago)
    {
        _context.Pagos.Remove(pago);
        await _context.SaveChangesAsync();
    }
}