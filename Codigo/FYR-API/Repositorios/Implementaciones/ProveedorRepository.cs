using Datos;
using DTO.Proveedor.Request;
using DTO.Proveedor.Response;
using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Repositorios.Interfaces;


namespace Repositorios.Implementaciones;

public class ProveedorRepository : IProveedorRepository
{
    private readonly AppDbContext _context;

    public ProveedorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProveedorResponse>> GetAllAsync()
    {
        return await _context.Proveedores
            .Select(p => new ProveedorResponse
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Telefono = p.Telefono,
                Correo = p.Correo,
                EmpresaId = p.EmpresaId
            })
            .ToListAsync();
    }

    public async Task<ProveedorResponse?> GetByIdAsync(int id)
    {
        return await _context.Proveedores
            .Where(p => p.Id == id)
            .Select(p => new ProveedorResponse
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Telefono = p.Telefono,
                Correo = p.Correo,
                EmpresaId = p.EmpresaId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProveedorResponse> CreateAsync(CreateProveedorRequest request)
    {
        var proveedor = new Proveedor
        {
            Nombre = request.Nombre,
            Telefono = request.Telefono,
            Correo = request.Correo,
            EmpresaId = request.EmpresaId
        };

        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();

        return new ProveedorResponse
        {
            Id = proveedor.Id,
            Nombre = proveedor.Nombre,
            Telefono = proveedor.Telefono,
            Correo = proveedor.Correo,
            EmpresaId = proveedor.EmpresaId
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateProveedorRequest request)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);

        if (proveedor == null)
            return false;

        proveedor.Nombre = request.Nombre;
        proveedor.Telefono = request.Telefono;
        proveedor.Correo = request.Correo;
        proveedor.EmpresaId = request.EmpresaId;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);

        if (proveedor == null)
            return false;

        _context.Proveedores.Remove(proveedor);
        await _context.SaveChangesAsync();

        return true;
    }
}