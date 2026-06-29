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
                ImagenUrl = p.ImagenUrl,
                Talles = p.Talles,
                Colores = p.Colores,
                Empresa = p.Empresa.NombreComercial,
                Categoria = p.Categoria.Nombre,
                Subcategoria = p.Subcategoria != null ? p.Subcategoria.Nombre : null
            })
            .ToListAsync();
    }

    public async Task<ProductoResponse?> GetByIdAsync(int id)
    {
        var p = await _context.Productos
            .Include(x => x.Empresa)
            .Include(x => x.Categoria)
            .Include(x => x.Subcategoria)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null)
            return null;

        return new ProductoResponse
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            ImagenUrl = p.ImagenUrl,
            Talles = p.Talles,
            Colores = p.Colores,
            Empresa = p.Empresa.NombreComercial,
            Categoria = p.Categoria.Nombre,
            Subcategoria = p.Subcategoria?.Nombre
        };
    }

    public async Task<ProductoResponse> CreateAsync(CreateProductoRequest request)
    {
        var producto = new Producto
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            ImagenUrl = request.ImagenUrl,
            Talles = request.Talles,
            Colores = request.Colores,
            EmpresaId = request.EmpresaId,
            CategoriaId = request.CategoriaId,
            SubcategoriaId = request.SubcategoriaId
        };

        _context.Productos.Add(producto);

        await _context.SaveChangesAsync();

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
        producto.ImagenUrl = request.ImagenUrl;
        producto.Talles = request.Talles;
        producto.Colores = request.Colores;
        producto.CategoriaId = request.CategoriaId;
        producto.SubcategoriaId = request.SubcategoriaId;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto == null)
            return false;

        _context.Productos.Remove(producto);

        await _context.SaveChangesAsync();

        return true;
    }
}