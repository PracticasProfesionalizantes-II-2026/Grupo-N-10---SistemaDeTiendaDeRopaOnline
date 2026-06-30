using Logica.DTO.Subcategoria.Requests;
using Logica.DTO.Subcategoria.Responses;

namespace Logica.Interfaces;

public interface ISubcategoriaService
{
    Task<List<SubcategoriaResponse>> GetAllAsync();

    Task<SubcategoriaResponse?> GetByIdAsync(int id);

    Task<SubcategoriaResponse> CreateAsync(CreateSubcategoriaRequest request);

    Task<bool> UpdateAsync(int id, UpdateSubcategoriaRequest request);

    Task<bool> DeleteAsync(int id);
}