using Entidades.Models;

namespace Repositorios.Interfaces;

public interface ISubcategoriaRepository
{
    Task<List<Subcategoria>> GetAllAsync();

    Task<Subcategoria?> GetByIdAsync(int id);

    Task<Subcategoria> AddAsync(Subcategoria subcategoria);

    Task UpdateAsync(Subcategoria subcategoria);

    Task DeleteAsync(Subcategoria subcategoria);
}