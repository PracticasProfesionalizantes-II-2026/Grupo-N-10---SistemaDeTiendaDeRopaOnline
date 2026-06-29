using Entidades.Models;
using Logica.DTO.Subcategoria.Requests;
using Logica.DTO.Subcategoria.Responses;
using Logica.Interfaces;
using Repositorios.Interfaces;

namespace Logica.Services;

public class SubcategoriaService : ISubcategoriaService
{
    private readonly ISubcategoriaRepository _repository;

    public SubcategoriaService(ISubcategoriaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SubcategoriaResponse>> GetAllAsync()
    {
        var lista = await _repository.GetAllAsync();

        return lista.Select(x => new SubcategoriaResponse
        {
            Id = x.Id,
            Nombre = x.Nombre,
            CategoriaId = x.CategoriaId
        }).ToList();
    }

    public async Task<SubcategoriaResponse?> GetByIdAsync(int id)
    {
        var subcategoria = await _repository.GetByIdAsync(id);

        if (subcategoria == null)
            return null;

        return new SubcategoriaResponse
        {
            Id = subcategoria.Id,
            Nombre = subcategoria.Nombre,
            CategoriaId = subcategoria.CategoriaId
        };
    }

    public async Task<SubcategoriaResponse> CreateAsync(CreateSubcategoriaRequest request)
    {
        var subcategoria = new Subcategoria
        {
            Nombre = request.Nombre,
            CategoriaId = request.CategoriaId
        };

        await _repository.AddAsync(subcategoria);

        return new SubcategoriaResponse
        {
            Id = subcategoria.Id,
            Nombre = subcategoria.Nombre,
            CategoriaId = subcategoria.CategoriaId
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateSubcategoriaRequest request)
    {
        var subcategoria = await _repository.GetByIdAsync(id);

        if (subcategoria == null)
            return false;

        subcategoria.Nombre = request.Nombre;
        subcategoria.CategoriaId = request.CategoriaId;

        await _repository.UpdateAsync(subcategoria);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var subcategoria = await _repository.GetByIdAsync(id);

        if (subcategoria == null)
            return false;

        await _repository.DeleteAsync(subcategoria);

        return true;
    }
}