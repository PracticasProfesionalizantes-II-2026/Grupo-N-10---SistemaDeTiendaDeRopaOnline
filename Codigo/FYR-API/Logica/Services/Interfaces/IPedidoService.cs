public interface IPedidoService
{
    Task<List<PedidoResponse>> GetAllAsync();
    Task<PedidoResponse?> GetByIdAsync(int id);
    Task<List<PedidoResponse>> GetByUsuarioAsync(int usuarioId);
    Task<PedidoResponse> CreateAsync(CreatePedidoRequest request);
    Task<PedidoResponse?> UpdateAsync(int id, UpdatePedidoRequest request);
    Task<PedidoResponse?> UpdateEstadoAsync(int id, UpdateEstadoPedidoRequest request);
    Task<PedidoResponse?> UpdateSeguimientoAsync(int id, UpdateSeguimientoPedidoRequest request);
    Task<bool> DeleteAsync(int id);
}