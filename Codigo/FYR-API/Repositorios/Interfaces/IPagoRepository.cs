using Entidades.Models;

public interface IPagoRepository
{
    Task<List<Pago>> GetByPedidoIdAsync(int pedidoId);
    Task<Pago?> GetByIdAsync(int pedidoId, int pagoId);
    Task AddAsync(Pago pago);
    Task UpdateAsync(Pago pago);
    Task DeleteAsync(Pago pago);
}