using Datos;
using DTO.Pago.Request;
using DTO.Pago.Response;
using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Repositorios.Interfaces;

namespace Repositorios.Implementaciones;

public class PagoRepository : IPagoRepository
{
    private readonly AppDbContext _context;

    public PagoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PagoResponse>> GetAllAsync()
    {
        return await _context.Pagos
            .Select(p => new PagoResponse
            {
                Id = p.Id,
                Monto = p.Monto,
                MetodoPago = p.MetodoPago,
                Estado = p.Estado,
                FechaPago = p.FechaPago,
                PedidoId = p.PedidoId
            })
            .ToListAsync();
    }

    public async Task<PagoResponse?> GetByIdAsync(int id)
    {
        return await _context.Pagos
            .Where(p => p.Id == id)
            .Select(p => new PagoResponse
            {
                Id = p.Id,
                Monto = p.Monto,
                MetodoPago = p.MetodoPago,
                Estado = p.Estado,
                FechaPago = p.FechaPago,
                PedidoId = p.PedidoId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PagoResponse> CreateAsync(CreatePagoRequest request)
    {
        var pago = new Pago
        {
            Monto = request.Monto,
            MetodoPago = request.MetodoPago,
            Estado = request.Estado,
            PedidoId = request.PedidoId,
            FechaPago = DateTime.UtcNow
        };

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        return new PagoResponse
        {
            Id = pago.Id,
            Monto = pago.Monto,
            MetodoPago = pago.MetodoPago,
            Estado = pago.Estado,
            FechaPago = pago.FechaPago,
            PedidoId = pago.PedidoId
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdatePagoRequest request)
    {
        var pago = await _context.Pagos.FindAsync(id);

        if (pago == null)
            return false;

        pago.Monto = request.Monto;
        pago.MetodoPago = request.MetodoPago;
        pago.Estado = request.Estado;
        pago.PedidoId = request.PedidoId;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var pago = await _context.Pagos.FindAsync(id);

        if (pago == null)
            return false;

        _context.Pagos.Remove(pago);
        await _context.SaveChangesAsync();

        return true;
    }
}