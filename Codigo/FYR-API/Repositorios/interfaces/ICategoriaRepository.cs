using Entidades.Models;

namespace Repositorios.Interfaces;

public interface ICategoriaRepository
{
    Task<List<Categoria>> GetAllAsync();

    Task<Categoria?> GetByIdAsync(int id);

    Task<Categoria> AddAsync(Categoria categoria);

    Task UpdateAsync(Categoria categoria);

    Task DeleteAsync(Categoria categoria);
}