using DTO.Producto.Requests;
using DTO.Producto.Responses;

namespace Repositorios.Interfaces;

public interface IProductoRepository
{
    Task<List<ProductoResponse>> GetAllAsync();

    Task<ProductoResponse?> GetByIdAsync(int id);

    Task<BuscarProductoResponse> SearchAsync(BuscarProductoRequest request);

    Task<ProductoResponse> CreateAsync(CreateProductoRequest request);

    Task<bool> UpdateAsync(int id, UpdateProductoRequest request);

    Task<bool> DeleteAsync(int id);
}