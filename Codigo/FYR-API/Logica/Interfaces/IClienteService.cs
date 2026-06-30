public interface IClienteService
{
    Task<List<ClienteResponse>> GetAllAsync();
    Task<ClienteResponse?> GetByIdAsync(int id);
    Task<ClienteResponse> CreateAsync(CreateClienteRequest request);
    Task<ClienteResponse?> UpdateAsync(int id, UpdateClienteRequest request);
    Task<bool> DeleteAsync(int id);
}