
using DTO.Producto.Requests;
using DTO.Producto.Responses;
using Repositorios.Interfaces;

namespace Logica.Services;
public class ProductoService
{
    private readonly IProductoRepository _productoRepository;

    public ProductoService(IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    public async Task<List<ProductoResponse>> GetAllAsync()
    {
        return await _productoRepository.GetAllAsync();
    }

    public async Task<ProductoResponse?> GetByIdAsync(int id)
    {
        return await _productoRepository.GetByIdAsync(id);
    }

    public async Task<ProductoResponse> CreateAsync(CreateProductoRequest request)
    {
        return await _productoRepository.CreateAsync(request);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductoRequest request)
    {
        return await _productoRepository.UpdateAsync(id, request);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _productoRepository.DeleteAsync(id);
    }
}
