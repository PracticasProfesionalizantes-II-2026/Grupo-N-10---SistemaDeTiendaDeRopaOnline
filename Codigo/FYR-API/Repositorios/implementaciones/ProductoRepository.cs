using Datos;
using DTO.Producto.Requests;
using DTO.Producto.Responses;
using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Repositorios.Interfaces;

namespace Repositorios.Implementaciones;

public class ProductoRepository : IProductoRepository
{
    private readonly AppDbContext _context;

    public ProductoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductoResponse>> GetAllAsync()
    {
        return await _context.Productos
            .Include(p => p.Empresa)
            .Include(p => p.Categoria)
            .Include(p => p.Subcategoria)
            .Select(p => new ProductoResponse
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                PrecioAnterior = p.PrecioAnterior,
                EsOferta = p.EsOferta,
                ImagenUrl = p.ImagenUrl,
                Talles = p.Talles,
                Colores = p.Colores,
                Empresa = p.Empresa.NombreComercial,
                EmpresaId = p.EmpresaId,
                Categoria = p.Categoria.Nombre,
                CategoriaId = p.CategoriaId,
                SubcategoriaId = p.SubcategoriaId,
                Subcategoria = p.Subcategoria != null ? p.Subcategoria.Nombre : null,
                Activo = p.Activo,
                Stock = p.Stocks.Sum(stock => stock.CantidadDisponible)
            })
            .ToListAsync();
    }

    public async Task<ProductoResponse?> GetByIdAsync(int id)
    {
        var p = await _context.Productos
            .Include(x => x.Empresa)
            .Include(x => x.Categoria)
            .Include(x => x.Subcategoria)
            .Include(x => x.Stocks)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null)
            return null;

        return new ProductoResponse
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            PrecioAnterior = p.PrecioAnterior,
            EsOferta = p.EsOferta,
            ImagenUrl = p.ImagenUrl,
            Talles = p.Talles,
            Colores = p.Colores,
            Empresa = p.Empresa.NombreComercial,
            EmpresaId = p.EmpresaId,
            Categoria = p.Categoria.Nombre,
            CategoriaId = p.CategoriaId,
            SubcategoriaId = p.SubcategoriaId,
            Subcategoria = p.Subcategoria?.Nombre,
            Activo = p.Activo,
            Stock = p.Stocks.Sum(stock => stock.CantidadDisponible)
        };
    }

    public async Task<ProductoResponse> CreateAsync(CreateProductoRequest request)
    {
        var producto = new Producto
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            PrecioAnterior = request.PrecioAnterior,
            EsOferta = request.EsOferta,
            ImagenUrl = request.ImagenUrl,
            Talles = request.Talles,
            Colores = request.Colores,
            EmpresaId = request.EmpresaId,
            CategoriaId = request.CategoriaId,
            SubcategoriaId = request.SubcategoriaId
        };

        _context.Productos.Add(producto);

        await _context.SaveChangesAsync();

        await GuardarStockAsync(producto.Id, request.EmpresaId, request.Stock);

        return (await GetByIdAsync(producto.Id))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductoRequest request)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto == null)
            return false;

        producto.Nombre = request.Nombre;
        producto.Descripcion = request.Descripcion;
        producto.Precio = request.Precio;
        producto.PrecioAnterior = request.PrecioAnterior;
        producto.EsOferta = request.EsOferta;
        producto.ImagenUrl = request.ImagenUrl;
        producto.Talles = request.Talles;
        producto.Colores = request.Colores;
        producto.CategoriaId = request.CategoriaId;
        producto.SubcategoriaId = request.SubcategoriaId;

        await _context.SaveChangesAsync();

        await GuardarStockAsync(producto.Id, null, request.Stock);

        return true;
    }

    private async Task GuardarStockAsync(int productoId, int? empresaId, int cantidad)
    {
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(item => item.ProductoId == productoId);

        if (stock is null)
        {
            var sucursal = await _context.Sucursales
                .FirstOrDefaultAsync(item => !empresaId.HasValue || item.EmpresaId == empresaId.Value);

            if (sucursal is null)
            {
                if (!empresaId.HasValue)
                    return;

                sucursal = new Sucursal
                {
                    Nombre = "Sucursal principal",
                    Direccion = "Sin especificar",
                    EmpresaId = empresaId.Value
                };
                _context.Sucursales.Add(sucursal);
                await _context.SaveChangesAsync();
            }

            stock = new Stock
            {
                ProductoId = productoId,
                SucursalId = sucursal.Id,
                CantidadDisponible = cantidad,
                Estado = cantidad > 0 ? Entidades.Enums.EstadoStock.Disponible : Entidades.Enums.EstadoStock.Agotado
            };
            _context.Stocks.Add(stock);
        }
        else
        {
            stock.CantidadDisponible = cantidad;
            stock.Estado = cantidad > 0 ? Entidades.Enums.EstadoStock.Disponible : Entidades.Enums.EstadoStock.Agotado;
            stock.FechaActualizacion = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto == null)
            return false;

        producto.Activo = false;

        await _context.SaveChangesAsync();

        return true;
    }
}