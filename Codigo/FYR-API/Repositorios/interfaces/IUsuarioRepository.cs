using Entidades.Models;

public interface IUsuarioRepository
{
    Task<List<Usuario>> GetAllAsync();
    Task<Usuario?> GetByIdAsync(int id);
    Task UpdateAsync(Usuario usuario);
    Task DeleteAsync(Usuario usuario);
    Task<Usuario> AddAsync(Usuario usuario);
}