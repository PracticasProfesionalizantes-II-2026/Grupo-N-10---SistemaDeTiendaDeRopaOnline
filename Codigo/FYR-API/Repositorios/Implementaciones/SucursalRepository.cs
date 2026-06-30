using Datos;
using DTO.Sucursal.Request;
using DTO.Sucursal.Response;
using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Repositorios.Interfaces;

namespace Repositorios.Implementaciones;

public class SucursalRepository : ISucursalRepository
{
    private readonly AppDbContext _context;

    public SucursalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SucursalResponse>> GetAllAsync()
    {
        return await _context.Sucursales
            .Select(s => new SucursalResponse
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Direccion = s.Direccion,
                Telefono = s.Telefono,
                Email = s.Email,
                EmpresaId = s.EmpresaId
            })
            .ToListAsync();
    }

    public async Task<SucursalResponse?> GetByIdAsync(int id)
    {
        return await _context.Sucursales
            .Where(s => s.Id == id)
            .Select(s => new SucursalResponse
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Direccion = s.Direccion,
                Telefono = s.Telefono,
                Email = s.Email,
                EmpresaId = s.EmpresaId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SucursalResponse> CreateAsync(CreateSucursalRequest request)
    {
        var sucursal = new Sucursal
        {
            Nombre = request.Nombre,
            Direccion = request.Direccion,
            Telefono = request.Telefono,
            Email = request.Email,
            EmpresaId = request.EmpresaId
        };

        _context.Sucursales.Add(sucursal);

        await _context.SaveChangesAsync();

        return new SucursalResponse
        {
            Id = sucursal.Id,
            Nombre = sucursal.Nombre,
            Direccion = sucursal.Direccion,
            Telefono = sucursal.Telefono,
            Email = sucursal.Email,
            EmpresaId = sucursal.EmpresaId
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateSucursalRequest request)
    {
        var sucursal = await _context.Sucursales.FindAsync(id);

        if (sucursal == null)
            return false;

        sucursal.Nombre = request.Nombre;
        sucursal.Direccion = request.Direccion;
        sucursal.Telefono = request.Telefono;
        sucursal.Email = request.Email;
        sucursal.EmpresaId = request.EmpresaId;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var sucursal = await _context.Sucursales.FindAsync(id);

        if (sucursal == null)
            return false;

        _context.Sucursales.Remove(sucursal);

        await _context.SaveChangesAsync();

        return true;
    }
}