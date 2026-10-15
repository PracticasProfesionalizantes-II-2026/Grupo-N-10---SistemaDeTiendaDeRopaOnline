using Entidades.Models;
using Entidades.Enums;
using Microsoft.EntityFrameworkCore;

namespace Datos;

public static class CatalogoInicialSeeder
{
    private sealed record ProductoInicial(
        string Nombre,
        string Imagen,
        string Genero,
        string Categoria,
        string Color,
        decimal Precio,
        bool EsOferta = false);

    private static readonly ProductoInicial[] Productos =
    {
        new("Abrigo North", "abrigo-hombre.jfif", "Hombre", "Abrigos", "Negro", 98000),
        new("Abrigo Urban", "abrigo-hombre-2.jfif", "Hombre", "Abrigos", "Gris", 112000, true),
        new("Abrigo Essential", "abrigo-hombre-3.webp", "Hombre", "Abrigos", "Beige", 105000),
        new("Buzo Street", "buzo-hombre3.webp", "Hombre", "Abrigos", "Negro", 85000),
        new("Buzo Comfy", "buzo4-hombre.webp", "Hombre", "Abrigos", "Verde", 79000),
        new("Buzo Classic", "buzo5-hombre.jfif", "Hombre", "Abrigos", "Azul", 82000),
        new("Campera Line", "campera2-hombre.webp", "Hombre", "Abrigos", "Marrón", 125000),
        new("Campera Denim", "camperahombre.jpg", "Hombre", "Abrigos", "Azul", 118000),
        new("Remera Basic", "remera-hombre4.jfif", "Hombre", "Remeras", "Blanco", 42000),
        new("Remera Sport", "remera7hombre.jfif", "Hombre", "Remeras", "Negro", 45000),
        new("Remera Essential", "remerahombre6.jfif", "Hombre", "Remeras", "Verde", 47000),
        new("Abrigo Soft", "abrigo-mujer.jfif", "Mujer", "Abrigos", "Beige", 99000),
        new("Abrigo Cozy", "abrigo-mujer-2.jpg", "Mujer", "Abrigos", "Gris", 108000),
        new("Abrigo Winter", "abrigo-mujer3.jfif", "Mujer", "Abrigos", "Negro", 115000, true),
        new("Buzo Cozy", "buzo-mujer.jfif", "Mujer", "Abrigos", "Rosa", 76000),
        new("Buzo Oversize", "buzo2-mujer.webp", "Mujer", "Abrigos", "Gris", 81000),
        new("Campera Puffer", "campera-mujer.webp", "Mujer", "Abrigos", "Negro", 128000),
        new("Campera Light", "campera-mujer2.webp", "Mujer", "Abrigos", "Verde", 119000),
        new("Jean Wide", "jeanmujer.jfif", "Mujer", "Pantalones", "Azul", 72000),
        new("Jean Straight", "jeanmujer2.webp", "Mujer", "Pantalones", "Celeste", 75000),
        new("Jogging Urban", "joggin-mujer.jfif", "Mujer", "Pantalones", "Gris", 52000, true),
        new("Cargo Relax", "pantalon-cargo-mujer.webp", "Mujer", "Pantalones", "Verde", 68000),
        new("Pollera Denim", "pollera-mujer2.jfif", "Mujer", "Polleras", "Azul", 56000),
        new("Pollera Black", "pollera-negra.jpg", "Mujer", "Polleras", "Negro", 58000),
        new("Remera Soft", "remera-mujer2.jfif", "Mujer", "Remeras", "Blanco", 39000),
        new("Remera Urban", "remera3-mujer.jpg", "Mujer", "Remeras", "Rojo", 43000),
        new("Remera Basic", "remera5-mujer.jfif", "Mujer", "Remeras", "Negro", 41000),
        new("Top Essential", "top-negro.webp", "Mujer", "Remeras", "Negro", 35000),
        new("Vestido Daily", "vestido-mujer.jfif", "Mujer", "Vestidos", "Negro", 85000),
        new("Vestido Flow", "vestido-mujer2.webp", "Mujer", "Vestidos", "Rojo", 92000),
        new("Vestido Midi", "vestido-mujer3.webp", "Mujer", "Vestidos", "Verde", 95000),
        new("Vestido Summer", "vestido-mujer4.webp", "Mujer", "Vestidos", "Blanco", 88000),
        new("Vestido Party", "vestidomujer5.jfif", "Mujer", "Vestidos", "Azul", 102000)
    };

    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        var empresa = await db.Empresas.FirstOrDefaultAsync();
        if (empresa is null)
        {
            empresa = new Empresa
            {
                NombreComercial = "F&R",
                LogoUrl = "/images/logo-fr.png",
                ColorPrimario = "#000000",
                Tipografia = "Arial",
                Cuit = "30-00000000-0",
                RazonSocial = "F&R",
                CondicionIVA = "Responsable Inscripto",
                Direccion = "Tienda F&R"
            };
            db.Empresas.Add(empresa);
            await db.SaveChangesAsync();
        }

        var categorias = await db.Categorias
            .Where(c => c.EmpresaId == empresa.Id)
            .ToListAsync();

        foreach (var nombreCategoria in Productos.Select(p => p.Categoria).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (categorias.Any(c => c.Nombre.Equals(nombreCategoria, StringComparison.OrdinalIgnoreCase)))
                continue;

            var categoria = new Categoria { Nombre = nombreCategoria, EmpresaId = empresa.Id };
            db.Categorias.Add(categoria);
            categorias.Add(categoria);
        }

        await db.SaveChangesAsync();

        var imagenesExistentes = await db.Productos
            .Where(p => p.ImagenUrl != null)
            .Select(p => p.ImagenUrl!)
            .ToListAsync();

        foreach (var productoInicial in Productos)
        {
            var imagenUrl = $"/images/{productoInicial.Imagen}";
            if (imagenesExistentes.Any(imagen =>
                    imagen.TrimStart('~').Equals(imagenUrl, StringComparison.OrdinalIgnoreCase)))
                continue;

            var categoria = categorias.First(c =>
                c.Nombre.Equals(productoInicial.Categoria, StringComparison.OrdinalIgnoreCase));
            var precio = productoInicial.EsOferta ? productoInicial.Precio * 0.8m : productoInicial.Precio;

            db.Productos.Add(new Producto
            {
                Nombre = productoInicial.Nombre,
                Descripcion = $"{productoInicial.Nombre} de colección F&R.",
                Precio = precio,
                PrecioAnterior = productoInicial.EsOferta ? productoInicial.Precio : null,
                EsOferta = productoInicial.EsOferta,
                ImagenUrl = imagenUrl,
                Talles = "S,M,L,XL",
                Colores = productoInicial.Color,
                EmpresaId = empresa.Id,
                CategoriaId = categoria.Id,
                Activo = true
            });

            imagenesExistentes.Add(imagenUrl);
        }

        await db.SaveChangesAsync();

        var clientesConNombreGenerico = await db.Usuarios
            .Where(u => u.Rol == Rol.Cliente &&
                        (string.IsNullOrWhiteSpace(u.Nombre) ||
                         EF.Functions.Like(u.Nombre, "%MOSTRADOR%") ||
                         EF.Functions.Like(u.Apellido, "%MOSTRADOR%")))
            .ToListAsync();

        foreach (var cliente in clientesConNombreGenerico)
        {
            cliente.Nombre = "María";
            cliente.Apellido = "González";
        }

        var clientesConNumeroEnApellido = await db.Usuarios
            .Where(u => u.Rol == Rol.Cliente)
            .ToListAsync();

        var clientesActualizados = clientesConNombreGenerico.Count > 0;
        foreach (var cliente in clientesConNumeroEnApellido)
        {
            var partesApellido = cliente.Apellido
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (partesApellido.Length > 1 &&
                int.TryParse(partesApellido[^1], out _))
            {
                cliente.Apellido = string.Join(' ', partesApellido[..^1]);
                clientesActualizados = true;
            }
        }

        if (clientesActualizados)
        {
            await db.SaveChangesAsync();
        }

        var proveedor = await db.Proveedores
            .FirstOrDefaultAsync(p => p.Correo == "ventas@textilrioparana.com");
        if (proveedor is null)
        {
            proveedor = new Proveedor
            {
                Nombre = "Carlos",
                Apellido = "Gómez",
                Telefono = "5491123456789",
                Correo = "ventas@textilrioparana.com",
                Direccion = "Av. Corrientes 2450, CABA",
                EmpresaId = empresa.Id
            };
            db.Proveedores.Add(proveedor);
            await db.SaveChangesAsync();
        }

        var sucursalPrincipal = await db.Sucursales
            .FirstOrDefaultAsync(s => s.EmpresaId == empresa.Id);
        if (sucursalPrincipal is null)
        {
            sucursalPrincipal = new Sucursal
            {
                Nombre = "Sucursal principal",
                Direccion = empresa.Direccion,
                EmpresaId = empresa.Id
            };
            db.Sucursales.Add(sucursalPrincipal);
            await db.SaveChangesAsync();
        }

        var productosPersistidos = await db.Productos
            .Where(p => p.EmpresaId == empresa.Id)
            .ToListAsync();
        foreach (var producto in productosPersistidos)
        {
            producto.Marca ??= "F&R";

            if (!await db.ProductosProveedor.AnyAsync(pp =>
                    pp.ProductoId == producto.Id && pp.ProveedorId == proveedor.Id))
            {
                db.ProductosProveedor.Add(new ProductoProveedor
                {
                    ProductoId = producto.Id,
                    ProveedorId = proveedor.Id,
                    PrecioCompra = Math.Round(producto.Precio * 0.55m, 2),
                    Cantidad = 10
                });
            }

            var stock = await db.Stocks.FirstOrDefaultAsync(s =>
                s.ProductoId == producto.Id && s.SucursalId == sucursalPrincipal.Id);
            if (stock is null)
            {
                db.Stocks.Add(new Stock
                {
                    ProductoId = producto.Id,
                    SucursalId = sucursalPrincipal.Id,
                    CantidadDisponible = 10,
                    Estado = EstadoStock.Disponible
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
