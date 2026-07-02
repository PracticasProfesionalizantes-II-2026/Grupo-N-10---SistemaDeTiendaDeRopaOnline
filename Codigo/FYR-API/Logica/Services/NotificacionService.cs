using Entidades.Models;

public class NotificacionService : INotificacionService
{
    private readonly INotificacionRepository _repository;

    public NotificacionService(INotificacionRepository repository)
    {
        _repository = repository;
    }

    private NotificacionResponse Map(Notificacion notificacion)
    {
        return new NotificacionResponse
        {
            IdNotificacion = notificacion.Id,
            Mensaje = notificacion.Mensaje,
            FechaEnvio = notificacion.FechaEnvio,
            Leida = notificacion.Leida,
            UsuarioId = notificacion.UsuarioId
        };
    }

    public async Task<List<NotificacionResponse>> GetAllAsync()
    {
        var notificaciones = await _repository.GetAllAsync();
        return notificaciones.Select(Map).ToList();
    }

    public async Task<NotificacionResponse?> GetByIdAsync(int id)
    {
        var notificacion = await _repository.GetByIdAsync(id);
        return notificacion == null ? null : Map(notificacion);
    }

    public async Task<NotificacionResponse> CreateAsync(CreateNotificacionRequest request)
    {
        var notificacion = new Notificacion
        {
            UsuarioId = request.UsuarioId,
            Mensaje = request.Mensaje,
            FechaEnvio = DateTime.UtcNow,
        };

        await _repository.AddAsync(notificacion);

        return Map(notificacion);
    }

    public async Task<NotificacionResponse?> UpdateEstadoAsync(int id, UpdateEstadoNotificacionRequest request)
    {
        var notificacion = await _repository.GetByIdAsync(id);

        if (notificacion == null)
            return null;

        notificacion.Leida = request.Leida;

        await _repository.UpdateAsync(notificacion);

        return Map(notificacion);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var notificacion = await _repository.GetByIdAsync(id);

        if (notificacion == null)
            return false;

        await _repository.DeleteAsync(notificacion);

        return true;
    }
}