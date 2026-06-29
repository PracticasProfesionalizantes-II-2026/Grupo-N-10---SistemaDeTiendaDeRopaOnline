using Datos;
using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Repositorios.Interfaces;

namespace Repositorios.Implementaciones;

public class SubcategoriaRepository : ISubcategoriaRepository
{
    private readonly AppDbContext _context;

    public SubcategoriaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Subcategoria>> GetAllAsync()
    {
        return await _context.Subcategorias
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Subcategoria?> GetByIdAsync(int id)
    {
        return await _context.Subcategorias
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Subcategoria> AddAsync(Subcategoria subcategoria)
    {
        _context.Subcategorias.Add(subcategoria);

        await _context.SaveChangesAsync();

        return subcategoria;
    }

    public async Task UpdateAsync(Subcategoria subcategoria)
    {
        _context.Subcategorias.Update(subcategoria);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Subcategoria subcategoria)
    {
        _context.Subcategorias.Remove(subcategoria);

        await _context.SaveChangesAsync();
    }
}