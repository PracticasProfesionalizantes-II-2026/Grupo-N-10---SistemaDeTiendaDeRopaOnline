using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Datos;
public class DetallePedidoRepository : IDetallePedidoRepository
{
    private readonly AppDbContext _context;

    public DetallePedidoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DetallePedido>> GetByPedidoIdAsync(int pedidoId)
    {
        return await _context.DetallesPedido
            .AsNoTracking()
            .Where(d => d.PedidoId == pedidoId)
            .ToListAsync();
    }

    public async Task<DetallePedido?> GetByIdAsync(int pedidoId, int detalleId)
    {
        return await _context.DetallesPedido
            .FirstOrDefaultAsync(d => d.PedidoId == pedidoId && d.Id == detalleId);
    }

    public async Task AddAsync(DetallePedido detalle)
    {
        await _context.DetallesPedido.AddAsync(detalle);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(DetallePedido detalle)
    {
        _context.DetallesPedido.Update(detalle);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(DetallePedido detalle)
    {
        _context.DetallesPedido.Remove(detalle);
        await _context.SaveChangesAsync();
    }
}