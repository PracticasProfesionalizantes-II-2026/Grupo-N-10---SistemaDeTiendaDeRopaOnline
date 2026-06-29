using Microsoft.EntityFrameworkCore;
using Entidades.Models;

namespace Datos;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // ==========================
    // DbSets
    // ==========================

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Subcategoria> Subcategorias { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<DetallePedido> DetallesPedido { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<Envio> Envios { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<ProductoProveedor> ProductosProveedor { get; set; }
    public DbSet<Reporte> Reportes { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<Sucursal> Sucursales { get; set; }
    public DbSet<Notificacion> Notificaciones { get; set; }
    public DbSet<MedioContacto> MediosContacto { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //==================================================
        // ÍNDICES ÚNICOS
        //==================================================

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Empresa>()
            .HasIndex(e => e.Cuit)
            .IsUnique();

        modelBuilder.Entity<Categoria>()
            .HasIndex(c => new { c.EmpresaId, c.Nombre })
            .IsUnique();

        modelBuilder.Entity<Subcategoria>()
            .HasIndex(s => new { s.CategoriaId, s.Nombre })
            .IsUnique();

        modelBuilder.Entity<Sucursal>()
            .HasIndex(s => new { s.EmpresaId, s.Nombre })
            .IsUnique();

        //==================================================
        // EMPRESA
        //==================================================

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Empresa)
            .WithMany(e => e.Usuarios)
            .HasForeignKey(u => u.EmpresaId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Categoria>()
            .HasOne(c => c.Empresa)
            .WithMany(e => e.Categorias)
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Proveedor>()
            .HasOne(p => p.Empresa)
            .WithMany(e => e.Proveedores)
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Sucursal>()
            .HasOne(s => s.Empresa)
            .WithMany(e => e.Sucursales)
            .HasForeignKey(s => s.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MedioContacto>()
            .HasOne(m => m.Empresa)
            .WithMany(e => e.MediosContacto)
            .HasForeignKey(m => m.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        //==================================================
        // CATEGORÍAS
        //==================================================

        modelBuilder.Entity<Subcategoria>()
            .HasOne(s => s.Categoria)
            .WithMany(c => c.Subcategorias)
            .HasForeignKey(s => s.CategoriaId)
            .OnDelete(DeleteBehavior.Cascade);

        //==================================================
        // PRODUCTOS
        //==================================================

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Empresa)
            .WithMany(e => e.Productos)
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Subcategoria)
            .WithMany(s => s.Productos)
            .HasForeignKey(p => p.SubcategoriaId)
            .OnDelete(DeleteBehavior.NoAction);

        //==================================================
        // PEDIDOS
        //==================================================

        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Usuario)
            .WithMany(u => u.Pedidos)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DetallePedido>()
            .HasOne(d => d.Pedido)
            .WithMany(p => p.DetallesPedido)
            .HasForeignKey(d => d.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DetallePedido>()
            .HasOne(d => d.Producto)
            .WithMany(p => p.DetallesPedido)
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Pago>()
            .HasOne(p => p.Pedido)
            .WithMany(pe => pe.Pagos)
            .HasForeignKey(p => p.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Pedido)
            .WithOne(p => p.Factura)
            .HasForeignKey<Factura>(f => f.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Envio>()
            .HasOne(e => e.Pedido)
            .WithOne(p => p.Envio)
            .HasForeignKey<Envio>(e => e.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        //==================================================
        // STOCK
        //==================================================

        modelBuilder.Entity<Stock>()
            .HasOne(s => s.Producto)
            .WithMany(p => p.Stocks)
            .HasForeignKey(s => s.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Stock>()
            .HasOne(s => s.Sucursal)
            .WithMany(su => su.Stocks)
            .HasForeignKey(s => s.SucursalId)
            .OnDelete(DeleteBehavior.Cascade);

        //==================================================
        // PRODUCTO - PROVEEDOR
        //==================================================

        modelBuilder.Entity<ProductoProveedor>()
            .HasOne(pp => pp.Producto)
            .WithMany(p => p.ProductosProveedor)
            .HasForeignKey(pp => pp.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductoProveedor>()
            .HasOne(pp => pp.Proveedor)
            .WithMany(p => p.ProductosProveedor)
            .HasForeignKey(pp => pp.ProveedorId)
            .OnDelete(DeleteBehavior.Cascade);

        //==================================================
        // REPORTES
        //==================================================

        modelBuilder.Entity<Reporte>()
            .HasOne(r => r.Usuario)
            .WithMany(u => u.Reportes)
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        //==================================================
        // NOTIFICACIONES
        //==================================================

        modelBuilder.Entity<Notificacion>()
            .HasOne(n => n.Usuario)
            .WithMany(u => u.Notificaciones)
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        //==================================================
        // DECIMALES
        //==================================================

        modelBuilder.Entity<Producto>()
            .Property(p => p.Precio)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Pedido>()
            .Property(p => p.Total)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Pago>()
            .Property(p => p.Monto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Factura>()
            .Property(f => f.Total)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePedido>()
            .Property(d => d.PrecioUnitario)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePedido>()
            .Property(d => d.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ProductoProveedor>()
            .Property(pp => pp.PrecioCompra)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Envio>()
            .Property(e => e.Costo)
            .HasPrecision(18, 2);
    }
}