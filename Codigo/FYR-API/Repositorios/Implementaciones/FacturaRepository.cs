using Datos;
using DTO.Factura.Request;
using DTO.Factura.Response;
using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Repositorios.Interfaces;

namespace Repositorios.Implementaciones;

public class FacturaRepository : IFacturaRepository
{
    private readonly AppDbContext _context;

    public FacturaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FacturaResponse>> GetAllAsync()
    {
        return await _context.Facturas
            .Select(f => new FacturaResponse
            {
                Id = f.Id,
                Fecha = f.Fecha,
                Tipo = f.Tipo,
                Numero = f.Numero,
                Total = f.Total,
                FormaPago = f.FormaPago,
                PedidoId = f.PedidoId
            })
            .ToListAsync();
    }

    public async Task<FacturaResponse?> GetByIdAsync(int id)
    {
        return await _context.Facturas
            .Where(f => f.Id == id)
            .Select(f => new FacturaResponse
            {
                Id = f.Id,
                Fecha = f.Fecha,
                Tipo = f.Tipo,
                Numero = f.Numero,
                Total = f.Total,
                FormaPago = f.FormaPago,
                PedidoId = f.PedidoId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<FacturaResponse> CreateAsync(CreateFacturaRequest request)
    {
        var factura = new Factura
        {
            Fecha = request.Fecha,
            Tipo = request.Tipo,
            Numero = request.Numero,
            Total = request.Total,
            FormaPago = request.FormaPago,
            PedidoId = request.PedidoId
        };

        _context.Facturas.Add(factura);
        await _context.SaveChangesAsync();

        return new FacturaResponse
        {
            Id = factura.Id,
            Fecha = factura.Fecha,
            Tipo = factura.Tipo,
            Numero = factura.Numero,
            Total = factura.Total,
            FormaPago = factura.FormaPago,
            PedidoId = factura.PedidoId
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateFacturaRequest request)
    {
        var factura = await _context.Facturas.FindAsync(id);

        if (factura == null)
            return false;

        factura.Fecha = request.Fecha;
        factura.Tipo = request.Tipo;
        factura.Numero = request.Numero;
        factura.Total = request.Total;
        factura.FormaPago = request.FormaPago;
        factura.PedidoId = request.PedidoId;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var factura = await _context.Facturas.FindAsync(id);

        if (factura == null)
            return false;

        _context.Facturas.Remove(factura);

        await _context.SaveChangesAsync();

        return true;
    }

    Task<IEnumerable<Factura>> IFacturaRepository.GetAllAsync()
    {
        throw new NotImplementedException();
    }

    Task<Factura?> IFacturaRepository.GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    Task<Factura> IFacturaRepository.CreateAsync(CreateFacturaRequest request)
    {
        throw new NotImplementedException();
    }
}