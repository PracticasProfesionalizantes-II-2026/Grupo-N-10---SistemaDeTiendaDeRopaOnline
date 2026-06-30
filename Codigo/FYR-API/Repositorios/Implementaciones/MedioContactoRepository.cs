using Datos;
using DTO.MedioContacto.Request;
using DTO.MedioContacto.Response;
using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Repositorios.Interfaces;

namespace Repositorios.Implementaciones;

public class MedioContactoRepository : IMedioContactoRepository
{
    private readonly AppDbContext _context;

    public MedioContactoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MedioContactoResponse>> GetAllAsync()
    {
        return await _context.MediosContacto
            .Select(m => new MedioContactoResponse
            {
                Id = m.Id,
                Medio = m.Medio,
                Valor = m.Valor,
                EmpresaId = m.EmpresaId
            })
            .ToListAsync();
    }

    public async Task<MedioContactoResponse?> GetByIdAsync(int id)
    {
        return await _context.MediosContacto
            .Where(m => m.Id == id)
            .Select(m => new MedioContactoResponse
            {
                Id = m.Id,
                Medio = m.Medio,
                Valor = m.Valor,
                EmpresaId = m.EmpresaId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<MedioContactoResponse> CreateAsync(CreateMedioContactoRequest request)
    {
        var medio = new MedioContacto
        {
            Medio = request.Medio,
            Valor = request.Valor,
            EmpresaId = request.EmpresaId
        };

        _context.MediosContacto.Add(medio);

        await _context.SaveChangesAsync();

        return new MedioContactoResponse
        {
            Id = medio.Id,
            Medio = medio.Medio,
            Valor = medio.Valor,
            EmpresaId = medio.EmpresaId
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateMedioContactoRequest request)
    {
        var medio = await _context.MediosContacto.FindAsync(id);

        if (medio == null)
            return false;

        medio.Medio = request.Medio;
        medio.Valor = request.Valor;
        medio.EmpresaId = request.EmpresaId;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var medio = await _context.MediosContacto.FindAsync(id);

        if (medio == null)
            return false;

        _context.MediosContacto.Remove(medio);

        await _context.SaveChangesAsync();

        return true;
    }
}