using DTO.Stock.Requests;
using DTO.Stock.Responses;

namespace Repositorios.Interfaces;

public interface IStockRepository
{
    Task<List<StockResponse>> GetAllAsync();

    Task<StockResponse?> GetByIdAsync(int id);

    Task<StockResponse> CreateAsync(CreateStockRequest request);

    Task<bool> UpdateAsync(int id, UpdateStockRequest request);

    Task<bool> DeleteAsync(int id);
}