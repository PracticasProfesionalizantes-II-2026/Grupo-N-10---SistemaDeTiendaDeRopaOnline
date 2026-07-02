using DTO.Usuario.Request;
public interface IUsuarioService
{
    Task<List<UsuarioResponse>> GetAllAsync();
    Task<UsuarioResponse?> GetByIdAsync(int id);
    Task<UsuarioResponse?> UpdateAsync(int id, UpdateUsuarioRequest request);
    Task<bool> DeleteAsync(int id);
    Task<UsuarioResponse> AddAsync(CreateUsuarioRequest request);
}