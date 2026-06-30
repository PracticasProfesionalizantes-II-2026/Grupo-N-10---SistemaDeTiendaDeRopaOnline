using Datos;
using DTO.Stock.Requests;
using DTO.Stock.Responses;
using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Repositorios.Interfaces;

namespace Repositorios.Implementaciones;

public class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;

    public StockRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockResponse>> GetAllAsync()
    {
        return await _context.Stocks
            .Include(s => s.Producto)
            .Include(s => s.Sucursal)
            .Select(s => new StockResponse
            {
                Id = s.Id,
                CantidadDisponible = s.CantidadDisponible,
                FechaActualizacion = s.FechaActualizacion,
                Estado = s.Estado.ToString(),
                ProductoId = s.ProductoId,
                Producto = s.Producto.Nombre,
                SucursalId = s.SucursalId,
                Sucursal = s.Sucursal.Nombre
            })
            .ToListAsync();
    }

    public async Task<StockResponse?> GetByIdAsync(int id)
    {
        var stock = await _context.Stocks
            .Include(s => s.Producto)
            .Include(s => s.Sucursal)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (stock == null)
            return null;

        return new StockResponse
        {
            Id = stock.Id,
            CantidadDisponible = stock.CantidadDisponible,
            FechaActualizacion = stock.FechaActualizacion,
            Estado = stock.Estado.ToString(),
            ProductoId = stock.ProductoId,
            Producto = stock.Producto.Nombre,
            SucursalId = stock.SucursalId,
            Sucursal = stock.Sucursal.Nombre
        };
    }

    public async Task<StockResponse> CreateAsync(CreateStockRequest request)
    {
        var stock = new Stock
        {
            CantidadDisponible = request.CantidadDisponible,
            FechaActualizacion = DateTime.UtcNow,
            Estado = request.Estado,
            ProductoId = request.ProductoId,
            SucursalId = request.SucursalId
        };

        _context.Stocks.Add(stock);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(stock.Id))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateStockRequest request)
    {
        var stock = await _context.Stocks.FindAsync(id);

        if (stock == null)
            return false;

        stock.CantidadDisponible = request.CantidadDisponible;
        stock.Estado = request.Estado;
        stock.SucursalId = request.SucursalId;
        stock.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var stock = await _context.Stocks.FindAsync(id);

        if (stock == null)
            return false;

        _context.Stocks.Remove(stock);

        await _context.SaveChangesAsync();

        return true;
    }
}