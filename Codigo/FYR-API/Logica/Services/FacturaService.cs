using Repositorios.Interfaces;
using DTO.Factura.Request;
using DTO.Factura.Response;
namespace Logica.Services;
public class FacturaService
{
    private readonly IFacturaRepository _facturaRepository;

    public FacturaService(IFacturaRepository facturaRepository)
    {
        _facturaRepository = facturaRepository;
    }

    public async Task<List<FacturaResponse>> GetAllAsync()
    {
        return await _facturaRepository.GetAllAsync();
    }

    public async Task<FacturaResponse?> GetByIdAsync(int id)
    {
        return await _facturaRepository.GetByIdAsync(id);
    }

    public async Task<FacturaResponse> CreateAsync(CreateFacturaRequest request)
    {
        return await _facturaRepository.CreateAsync(request);
    }

    public async Task<bool> UpdateAsync(int id, UpdateFacturaRequest request)
    {
        return await _facturaRepository.UpdateAsync(id, request);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _facturaRepository.DeleteAsync(id);
    }
}
