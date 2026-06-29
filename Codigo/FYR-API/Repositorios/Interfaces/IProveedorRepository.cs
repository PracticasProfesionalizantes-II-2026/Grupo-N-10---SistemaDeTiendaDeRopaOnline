using DTO.Proveedor.Request;
using DTO.Proveedor.Response;
using Entidades.Models;

namespace Repositorios.Interfaces;

public interface IProveedorRepository
{
    Task<List<ProveedorResponse>> GetAllAsync();

    Task<ProveedorResponse?> GetByIdAsync(int id);

    Task<ProveedorResponse> CreateAsync(CreateProveedorRequest request);

    Task<bool> UpdateAsync(int id, UpdateProveedorRequest request);

    Task<bool> DeleteAsync(int id);
}