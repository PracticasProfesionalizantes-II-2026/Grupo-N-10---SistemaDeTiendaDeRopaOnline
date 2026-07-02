using Entidades.Models;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    private ClienteResponse Map(Usuario usuario)
    {
        return new ClienteResponse
        {
            IdUsuario = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            Telefono = usuario.Telefono,
            IdiomaPreferido = usuario.IdiomaPreferido,
            Activo = usuario.Activo,
            FotoPerfil = usuario.FotoPerfil
        };
    }

    public async Task<List<ClienteResponse>> GetAllAsync()
    {
        var clientes = await _repository.GetAllAsync();
        return clientes.Select(Map).ToList();
    }

    public async Task<ClienteResponse?> GetByIdAsync(int id)
    {
        var cliente = await _repository.GetByIdAsync(id);
        return cliente == null ? null : Map(cliente);
    }

    public async Task<ClienteResponse> CreateAsync(CreateClienteRequest request)
    {
        var cliente = new Usuario
        {
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            PasswordHash = request.PasswordHash,
            Telefono = request.Telefono,
            IdiomaPreferido = request.IdiomaPreferido,
            Rol = Rol.Cliente
        };

        await _repository.AddAsync(cliente);

        return Map(cliente);
    }

    public async Task<ClienteResponse?> UpdateAsync(int id, UpdateClienteRequest request)
    {
        var cliente = await _repository.GetByIdAsync(id);

        if (cliente == null)
            return null;

        cliente.Telefono = request.Telefono;
        cliente.IdiomaPreferido = request.IdiomaPreferido;
        cliente.Activo = request.Activo;
        cliente.FotoPerfil = request.FotoPerfil;

        await _repository.UpdateAsync(cliente);

        return Map(cliente);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var cliente = await _repository.GetByIdAsync(id);

        if (cliente == null)
            return false;

        await _repository.DeleteAsync(cliente);

        return true;
    }
}