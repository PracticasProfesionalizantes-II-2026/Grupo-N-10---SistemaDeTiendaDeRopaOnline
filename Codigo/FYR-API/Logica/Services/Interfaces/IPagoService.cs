public interface IPagoService
{
    Task<List<PagoResponse>> GetByPedidoAsync(int pedidoId);
    Task<PagoResponse?> GetByIdAsync(int pedidoId, int pagoId);
    Task<PagoResponse> CreateAsync(int pedidoId, CreatePagoRequest request);
    Task<PagoResponse?> UpdateAsync(int pedidoId, int pagoId, UpdatePagoRequest request);
    Task<bool> DeleteAsync(int pedidoId, int pagoId);
}