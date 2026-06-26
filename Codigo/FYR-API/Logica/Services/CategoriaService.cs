using Entidades.Models;
using Logica.DTO.Categoria.Requests;
using Logica.DTO.Categoria.Responses;
using Logica.Interfaces;
using Repository.Interfaces;

namespace Logica.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _repository;

    public CategoriaService(ICategoriaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CategoriaResponse>> GetAllAsync()
    {
        var categorias = await _repository.GetAllAsync();

        return categorias.Select(c => new CategoriaResponse
        {
            Id = c.Id,
            Nombre = c.Nombre,
            EmpresaId = c.EmpresaId
        }).ToList();
    }

    public async Task<CategoriaResponse?> GetByIdAsync(int id)
    {
        var categoria = await _repository.GetByIdAsync(id);

        if (categoria == null)
            return null;

        return new CategoriaResponse
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            EmpresaId = categoria.EmpresaId
        };
    }

    public async Task<CategoriaResponse> CreateAsync(CreateCategoriaRequest request)
    {
        var categoria = new Entidades.Models.Categoria
        {
            Nombre = request.Nombre,
            EmpresaId = request.EmpresaId
        };

        await _repository.AddAsync(categoria);

        return new CategoriaResponse
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            EmpresaId = categoria.EmpresaId
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateCategoriaRequest request)
    {
        var categoria = await _repository.GetByIdAsync(id);

        if (categoria == null)
            return false;

        categoria.Nombre = request.Nombre;
        categoria.EmpresaId = request.EmpresaId;

        await _repository.UpdateAsync(categoria);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var categoria = await _repository.GetByIdAsync(id);

        if (categoria == null)
            return false;

        await _repository.DeleteAsync(categoria);

        return true;
    }
}