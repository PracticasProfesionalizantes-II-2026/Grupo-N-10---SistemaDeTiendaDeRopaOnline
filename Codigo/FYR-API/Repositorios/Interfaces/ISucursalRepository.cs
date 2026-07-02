using DTO.Sucursal.Request;
using DTO.Sucursal.Response;

namespace Repositorios.Interfaces;

public interface ISucursalRepository
{
    Task<List<SucursalResponse>> GetAllAsync();

    Task<SucursalResponse?> GetByIdAsync(int id);

    Task<SucursalResponse> CreateAsync(CreateSucursalRequest request);

    Task<bool> UpdateAsync(int id, UpdateSucursalRequest request);

    Task<bool> DeleteAsync(int id);
}