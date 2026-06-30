using DTO.Factura.Request;
using DTO.Factura.Response;

namespace Repositorios.Interfaces;

public interface IFacturaRepository
{
    Task<List<FacturaResponse>> GetAllAsync();

    Task<FacturaResponse?> GetByIdAsync(int id);

    Task<FacturaResponse> CreateAsync(CreateFacturaRequest request);

    Task<bool> UpdateAsync(int id, UpdateFacturaRequest request);

    Task<bool> DeleteAsync(int id);
}