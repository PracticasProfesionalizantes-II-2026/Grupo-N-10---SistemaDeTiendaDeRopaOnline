using Entidades.Models;
public interface IDetallePedidoRepository
{
    Task<List<DetallePedido>> GetByPedidoIdAsync(int pedidoId);
    Task<DetallePedido?> GetByIdAsync(int pedidoId, int detalleId);
    Task AddAsync(DetallePedido detalle);
    Task UpdateAsync(DetallePedido detalle);
    Task DeleteAsync(DetallePedido detalle);
}