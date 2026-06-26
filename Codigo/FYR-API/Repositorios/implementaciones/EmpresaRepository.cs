using Datos;
using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;

namespace Repository;

public class EmpresaRepository : IEmpresaRepository
{
    private readonly AppDbContext _context;

    public EmpresaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Empresa>> GetAllAsync()
    {
        return await _context.Empresas
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Empresa?> GetByIdAsync(int id)
    {
        return await _context.Empresas
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Empresa> AddAsync(Empresa empresa)
    {
        _context.Empresas.Add(empresa);

        await _context.SaveChangesAsync();

        return empresa;
    }

    public async Task UpdateAsync(Empresa empresa)
    {
        _context.Empresas.Update(empresa);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Empresa empresa)
    {
        _context.Empresas.Remove(empresa);

        await _context.SaveChangesAsync();
    }
  
    public async Task<Empresa?> GetByCuitAsync(string cuit)
{
    return await _context.Empresas
        .FirstOrDefaultAsync(e => e.Cuit == cuit);
}

public async Task SaveChangesAsync()
{
    await _context.SaveChangesAsync();
}
}