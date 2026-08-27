using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Datos;

public class PedidoRepository : IPedidoRepository
{
    private readonly AppDbContext _context;

    public PedidoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Pedido>> GetAllAsync()
    {
        return await _context.Pedidos
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Pedido?> GetByIdAsync(int id)
    {
        return await _context.Pedidos
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Pedido>> GetByUsuarioIdAsync(int usuarioId)
    {
        return await _context.Pedidos
            .AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync();
    }

    public async Task AddAsync(Pedido pedido)
    {
        await _context.Pedidos.AddAsync(pedido);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Pedido pedido)
    {
        _context.Pedidos.Update(pedido);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Pedido pedido)
    {
        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();
    }
}