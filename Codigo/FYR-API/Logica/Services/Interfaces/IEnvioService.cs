public interface IEnvioService
{
    Task<List<EnvioResponse>> GetByPedidoAsync(int pedidoId);
    Task<EnvioResponse?> GetByIdAsync(int pedidoId, int envioId);
    Task<EnvioResponse> CreateAsync(int pedidoId, CreateEnvioRequest request);
    Task<EnvioResponse?> UpdateAsync(int pedidoId, int envioId, UpdateEnvioRequest request);
    Task<EnvioResponse?> UpdateEstadoAsync(int pedidoId, int envioId, UpdateEstadoEnvioRequest request);
    Task<bool> DeleteAsync(int pedidoId, int envioId);
    Task<CalcularEnvioResponse> CalcularAsync(CalcularEnvioRequest request);
    Task<EnvioResponse?> GetTrackingAsync(int envioId);
}