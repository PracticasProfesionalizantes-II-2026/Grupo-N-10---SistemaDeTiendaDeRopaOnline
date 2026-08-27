using DTO.Factura.Request;
using DTO.Factura.Response;
namespace Logica.Interfaces;
public interface IFacturaService
{
    Task<List<FacturaResponse>> GetAllAsync();

    Task<FacturaResponse?> GetByIdAsync(int id);

    Task<FacturaResponse> CreateAsync(CreateFacturaRequest request);

    Task<bool> UpdateAsync(int id, UpdateFacturaRequest request);

    Task<bool> DeleteAsync(int id);
}