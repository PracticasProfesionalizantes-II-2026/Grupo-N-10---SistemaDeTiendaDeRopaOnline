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
            .Include(p => p.ProductosProveedor)
                .ThenInclude(pp => pp.Proveedor)
            .Select(p => new ProductoResponse
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Marca = p.Marca,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                PrecioAnterior = p.PrecioAnterior,
                EsOferta = p.EsOferta,
                ImagenUrl = p.ImagenUrl,
                Talles = p.Talles,
                Colores = p.Colores,
                Genero = p.Genero,
                Empresa = p.Empresa.NombreComercial,
                EmpresaId = p.EmpresaId,
                Categoria = p.Categoria.Nombre,
                CategoriaId = p.CategoriaId,
                SubcategoriaId = p.SubcategoriaId,
                Subcategoria = p.Subcategoria != null ? p.Subcategoria.Nombre : null,
                Activo = p.Activo,
                Stock = p.Stocks.Sum(stock => stock.CantidadDisponible)
                    ,
                ProveedorId = p.ProductosProveedor.OrderByDescending(pp => pp.FechaAlta).Select(pp => (int?)pp.ProveedorId).FirstOrDefault(),
                ProveedorNombre = p.ProductosProveedor.OrderByDescending(pp => pp.FechaAlta).Select(pp => pp.Proveedor.Nombre).FirstOrDefault(),
                PrecioCompra = p.ProductosProveedor.OrderByDescending(pp => pp.FechaAlta).Select(pp => (decimal?)pp.PrecioCompra).FirstOrDefault()
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
            .Include(x => x.ProductosProveedor)
                .ThenInclude(pp => pp.Proveedor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null)
            return null;

        return new ProductoResponse
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Marca = p.Marca,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            PrecioAnterior = p.PrecioAnterior,
            EsOferta = p.EsOferta,
            ImagenUrl = p.ImagenUrl,
            Talles = p.Talles,
            Colores = p.Colores,
            Genero = p.Genero,
            Empresa = p.Empresa.NombreComercial,
            EmpresaId = p.EmpresaId,
            Categoria = p.Categoria.Nombre,
            CategoriaId = p.CategoriaId,
            SubcategoriaId = p.SubcategoriaId,
            Subcategoria = p.Subcategoria?.Nombre,
            Activo = p.Activo,
            Stock = p.Stocks.Sum(stock => stock.CantidadDisponible)
                ,
            ProveedorId = p.ProductosProveedor.OrderByDescending(pp => pp.FechaAlta).Select(pp => (int?)pp.ProveedorId).FirstOrDefault(),
            ProveedorNombre = p.ProductosProveedor.OrderByDescending(pp => pp.FechaAlta).Select(pp => pp.Proveedor.Nombre).FirstOrDefault(),
            PrecioCompra = p.ProductosProveedor.OrderByDescending(pp => pp.FechaAlta).Select(pp => (decimal?)pp.PrecioCompra).FirstOrDefault()
        };
    }

    public async Task<BuscarProductoResponse> SearchAsync(BuscarProductoRequest request)
    {
        var query = _context.Productos
            .AsNoTracking()
            .Where(producto => request.IncluirInactivos || producto.Activo);

        if (!string.IsNullOrWhiteSpace(request.Texto))
        {
            var texto = request.Texto.Trim();
            query = query.Where(producto =>
                EF.Functions.Like(producto.Nombre, $"%{texto}%") ||
                (producto.Marca != null && EF.Functions.Like(producto.Marca, $"%{texto}%")) ||
                (producto.Descripcion != null && EF.Functions.Like(producto.Descripcion, $"%{texto}%")) ||
                (producto.Colores != null && EF.Functions.Like(producto.Colores, $"%{texto}%")) ||
                (producto.Talles != null && EF.Functions.Like(producto.Talles, $"%{texto}%")) ||
                (producto.Genero != null && EF.Functions.Like(producto.Genero, $"%{texto}%")));
        }

        if (request.CategoriaId.HasValue)
            query = query.Where(producto => producto.CategoriaId == request.CategoriaId.Value);

        if (request.SubcategoriaId.HasValue)
            query = query.Where(producto => producto.SubcategoriaId == request.SubcategoriaId.Value);

        if (!string.IsNullOrWhiteSpace(request.Genero))
            query = query.Where(producto => producto.Genero != null &&
                EF.Functions.Like(producto.Genero, $"%{request.Genero.Trim()}%"));

        if (!string.IsNullOrWhiteSpace(request.Color))
            query = query.Where(producto => producto.Colores != null &&
                EF.Functions.Like(producto.Colores, $"%{request.Color.Trim()}%"));

        if (!string.IsNullOrWhiteSpace(request.Talle))
            query = query.Where(producto => producto.Talles != null &&
                EF.Functions.Like(producto.Talles, $"%{request.Talle.Trim()}%"));

        if (request.PrecioMinimo.HasValue)
            query = query.Where(producto => producto.Precio >= request.PrecioMinimo.Value);

        if (request.PrecioMaximo.HasValue)
            query = query.Where(producto => producto.Precio <= request.PrecioMaximo.Value);

        if (request.SoloOfertas == true)
            query = query.Where(producto => producto.EsOferta);

        if (request.SoloDisponibles == true)
            query = query.Where(producto => producto.Stocks.Sum(stock => stock.CantidadDisponible) > 0);

        var total = await query.CountAsync();
        var productos = await query
            .OrderBy(producto => producto.Nombre)
            .ThenBy(producto => producto.Id)
            .Skip((request.Pagina - 1) * request.TamanoPagina)
            .Take(request.TamanoPagina)
            .Select(p => new ProductoResponse
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Marca = p.Marca,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                PrecioAnterior = p.PrecioAnterior,
                EsOferta = p.EsOferta,
                ImagenUrl = p.ImagenUrl,
                Talles = p.Talles,
                Colores = p.Colores,
                Genero = p.Genero,
                Empresa = p.Empresa.NombreComercial,
                EmpresaId = p.EmpresaId,
                Categoria = p.Categoria.Nombre,
                CategoriaId = p.CategoriaId,
                SubcategoriaId = p.SubcategoriaId,
                Subcategoria = p.Subcategoria != null ? p.Subcategoria.Nombre : null,
                Activo = p.Activo,
                Stock = p.Stocks.Sum(stock => stock.CantidadDisponible),
                ProveedorId = p.ProductosProveedor.OrderByDescending(pp => pp.FechaAlta)
                    .Select(pp => (int?)pp.ProveedorId).FirstOrDefault(),
                ProveedorNombre = p.ProductosProveedor.OrderByDescending(pp => pp.FechaAlta)
                    .Select(pp => pp.Proveedor.Nombre).FirstOrDefault(),
                PrecioCompra = p.ProductosProveedor.OrderByDescending(pp => pp.FechaAlta)
                    .Select(pp => (decimal?)pp.PrecioCompra).FirstOrDefault()
            })
            .ToListAsync();

        return new BuscarProductoResponse
        {
            Productos = productos,
            Total = total,
            Pagina = request.Pagina,
            TamanoPagina = request.TamanoPagina,
            TotalPaginas = total == 0
                ? 0
                : (int)Math.Ceiling(total / (double)request.TamanoPagina)
        };
    }

    public async Task<ProductoResponse> CreateAsync(CreateProductoRequest request)
    {
        var producto = new Producto
        {
            Nombre = request.Nombre,
            Marca = request.Marca,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            PrecioAnterior = request.PrecioAnterior,
            EsOferta = request.EsOferta,
            ImagenUrl = request.ImagenUrl,
            Talles = request.Talles,
            Colores = request.Colores,
            Genero = request.Genero,
            EmpresaId = request.EmpresaId,
            CategoriaId = request.CategoriaId,
            SubcategoriaId = request.SubcategoriaId
        };

        _context.Productos.Add(producto);

        await _context.SaveChangesAsync();

        await GuardarStockAsync(producto.Id, request.EmpresaId, request.Stock);
        await GuardarProveedorAsync(producto.Id, request.ProveedorId, request.PrecioCompra);

        return (await GetByIdAsync(producto.Id))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductoRequest request)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto == null)
            return false;

        producto.Nombre = request.Nombre;
        producto.Marca = request.Marca;
        producto.Descripcion = request.Descripcion;
        producto.Precio = request.Precio;
        producto.PrecioAnterior = request.PrecioAnterior;
        producto.EsOferta = request.EsOferta;
        producto.ImagenUrl = request.ImagenUrl;
        producto.Talles = request.Talles;
        producto.Colores = request.Colores;
        producto.Genero = request.Genero;
        producto.CategoriaId = request.CategoriaId;
        producto.SubcategoriaId = request.SubcategoriaId;

        await _context.SaveChangesAsync();

        await GuardarStockAsync(producto.Id, null, request.Stock);
        await GuardarProveedorAsync(producto.Id, request.ProveedorId, request.PrecioCompra);

        return true;
    }

    private async Task GuardarProveedorAsync(int productoId, int? proveedorId, decimal? precioCompra)
    {
        var relacion = await _context.ProductosProveedor
            .OrderByDescending(pp => pp.FechaAlta)
            .FirstOrDefaultAsync(pp => pp.ProductoId == productoId);

        if (!proveedorId.HasValue)
        {
            if (relacion is not null)
            {
                _context.ProductosProveedor.Remove(relacion);
                await _context.SaveChangesAsync();
            }

            return;
        }

        if (relacion is null)
        {
            relacion = new ProductoProveedor { ProductoId = productoId, ProveedorId = proveedorId.Value };
            _context.ProductosProveedor.Add(relacion);
        }
        else
        {
            relacion.ProveedorId = proveedorId.Value;
        }

        if (precioCompra.HasValue)
            relacion.PrecioCompra = precioCompra.Value;

        await _context.SaveChangesAsync();
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