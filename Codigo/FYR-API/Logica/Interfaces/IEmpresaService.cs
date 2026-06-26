using Logica.DTO.Empresa.Request;
using Logica.DTO.Empresa.Response;

namespace Logica.Interfaces;

public interface IEmpresaService
{
    Task<List<EmpresaResponse>> GetAllAsync();

    Task<EmpresaResponse?> GetByIdAsync(int id);

    Task<EmpresaResponse> GetByCuitAsync(string cuit);

    Task<EmpresaResponse> CrearAsync(CrearEmpresaRequest request);

    Task<bool> ActualizarAsync(int id, ActualizarEmpresaRequest request);

    Task<bool> EliminarAsync(int id);
}