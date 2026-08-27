public interface INotificacionService
{
    Task<List<NotificacionResponse>> GetAllAsync();
    Task<NotificacionResponse?> GetByIdAsync(int id);
    Task<NotificacionResponse> CreateAsync(CreateNotificacionRequest request);
    Task<NotificacionResponse?> UpdateEstadoAsync(int id, UpdateEstadoNotificacionRequest request);
    Task<bool> DeleteAsync(int id);
}