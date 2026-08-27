public interface IDetallePedidoService
{
    Task<List<DetallePedidoResponse>> GetByPedidoAsync(int pedidoId);
    Task<DetallePedidoResponse?> GetByIdAsync(int pedidoId, int detalleId);
    Task<DetallePedidoResponse> CreateAsync(int pedidoId, CreateDetallePedidoRequest request);
    Task<DetallePedidoResponse?> UpdateAsync(int pedidoId, int detalleId, UpdateDetallePedidoRequest request);
    Task<bool> DeleteAsync(int pedidoId, int detalleId);
}