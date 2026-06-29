using DTO.Factura.Request;
using Entidades.Models;
using Repositorios.Interfaces;


namespace Repositorios.Interfaces;

public interface IFacturaRepository
{
    Task<IEnumerable<Factura>> GetAllAsync();
    Task<Factura?> GetByIdAsync(int id);
    Task<Factura> CreateAsync(CreateFacturaRequest request);
    Task<bool> UpdateAsync(int id, UpdateFacturaRequest request);
    Task<bool> DeleteAsync(int id);
}