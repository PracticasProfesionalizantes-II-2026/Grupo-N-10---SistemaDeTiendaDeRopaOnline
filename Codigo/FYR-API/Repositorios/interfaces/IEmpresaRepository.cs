using Entidades.Models;

namespace Repository.Interfaces;

public interface IEmpresaRepository
{
    Task<List<Empresa>> GetAllAsync();

    Task<Empresa?> GetByIdAsync(int id);

    Task<Empresa?> GetByCuitAsync(string cuit);

    Task<Empresa> AddAsync(Empresa empresa);

    Task UpdateAsync(Empresa empresa);

    Task DeleteAsync(Empresa empresa);

    Task SaveChangesAsync();
}