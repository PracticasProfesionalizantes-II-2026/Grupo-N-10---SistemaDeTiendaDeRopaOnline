using DTO.Usuario.Request;
using DTO.Usuario.Response;

namespace Repositorios.Interfaces;

public interface IUsuarioRepository
{
    Task<List<UsuarioResponse>> GetAllAsync();

    Task<UsuarioResponse?> GetByIdAsync(int id);

    Task<UsuarioResponse> CreateAsync(CreateUsuarioRequest request);

    Task<bool> UpdateAsync(int id, UpdateUsuarioRequest request);

    Task<bool> DeleteAsync(int id);
}