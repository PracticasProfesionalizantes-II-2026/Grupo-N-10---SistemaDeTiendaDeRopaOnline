using DTO.MedioContacto.Request;
using DTO.MedioContacto.Response;

namespace Repositorios.Interfaces;

public interface IMedioContactoRepository
{
    Task<List<MedioContactoResponse>> GetAllAsync();

    Task<MedioContactoResponse?> GetByIdAsync(int id);

    Task<MedioContactoResponse> CreateAsync(CreateMedioContactoRequest request);

    Task<bool> UpdateAsync(int id, UpdateMedioContactoRequest request);

    Task<bool> DeleteAsync(int id);
}