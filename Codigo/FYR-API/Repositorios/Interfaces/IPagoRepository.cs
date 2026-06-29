using DTO.Pago.Request;
using DTO.Pago.Response;

namespace Repositorios.Interfaces;

public interface IPagoRepository
{
    Task<List<PagoResponse>> GetAllAsync();

    Task<PagoResponse?> GetByIdAsync(int id);

    Task<PagoResponse> CreateAsync(CreatePagoRequest request);

    Task<bool> UpdateAsync(int id, UpdatePagoRequest request);

    Task<bool> DeleteAsync(int id);
}