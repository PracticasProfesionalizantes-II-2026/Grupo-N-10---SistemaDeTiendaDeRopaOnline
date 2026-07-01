using Entidades.Models;
using DTO.Usuario.Request;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;

    public UsuarioService(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    private UsuarioResponse Map(Usuario usuario)
    {
        return new UsuarioResponse
        {
            IdUsuario = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            TipoUsuario = usuario.Rol.ToString(),
            Telefono = usuario.Telefono,
            IdiomaPreferido = usuario.IdiomaPreferido
        };
    }

    public async Task<List<UsuarioResponse>> GetAllAsync()
    {
        var usuarios = await _repository.GetAllAsync();
        return usuarios.Select(Map).ToList();
    }

    public async Task<UsuarioResponse?> GetByIdAsync(int id)
    {
        var usuario = await _repository.GetByIdAsync(id);

        return usuario == null ? null : Map(usuario);
    }

    public async Task<UsuarioResponse?> UpdateAsync(int id, UpdateUsuarioRequest request)
    {
        var usuario = await _repository.GetByIdAsync(id);

        if (usuario == null) return null;

        usuario.Nombre = request.Nombre;
        usuario.Apellido = request.Apellido;
        usuario.Telefono = request.Telefono;
        usuario.IdiomaPreferido = request.IdiomaPreferido;

        await _repository.UpdateAsync(usuario);

        return Map(usuario);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var usuario = await _repository.GetByIdAsync(id);

        if (usuario == null) return false;

        await _repository.DeleteAsync(usuario);


        return true;
    }
    public async Task<UsuarioResponse> AddAsync(CreateUsuarioRequest request)
    {
        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            Rol = request.Rol,
            Telefono = request.Telefono,
            IdiomaPreferido = request.IdiomaPreferido,
            FotoPerfil = request.FotoPerfil,
            Activo = request.Activo,
            EmpresaId = request.EmpresaId
        };

        await _repository.AddAsync(usuario);

        return Map(usuario);
    }
}