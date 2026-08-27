using Entidades.Models;

public interface IEnvioRepository
{
    Task<List<Envio>> GetByPedidoIdAsync(int pedidoId);
    Task<Envio?> GetByIdAsync(int pedidoId, int envioId);
    Task<Envio?> GetTrackingAsync(int envioId);
    Task AddAsync(Envio envio);
    Task UpdateAsync(Envio envio);
    Task DeleteAsync(Envio envio);
}