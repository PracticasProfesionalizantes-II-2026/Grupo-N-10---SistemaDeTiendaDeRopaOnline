using Entidades.Models;
public interface IPedidoRepository
{
    Task<List<Pedido>> GetAllAsync();
    Task<Pedido?> GetByIdAsync(int id);
    Task<List<Pedido>> GetByUsuarioIdAsync(int usuarioId);
    Task AddAsync(Pedido pedido);
    Task UpdateAsync(Pedido pedido);
    Task DeleteAsync(Pedido pedido);
}