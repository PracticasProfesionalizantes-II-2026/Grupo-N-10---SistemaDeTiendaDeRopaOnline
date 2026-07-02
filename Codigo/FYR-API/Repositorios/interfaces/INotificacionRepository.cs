using Entidades.Models;

public interface INotificacionRepository
{
    Task<List<Notificacion>> GetAllAsync();
    Task<Notificacion?> GetByIdAsync(int id);
    Task AddAsync(Notificacion notificacion);
    Task UpdateAsync(Notificacion notificacion);
    Task DeleteAsync(Notificacion notificacion);
}