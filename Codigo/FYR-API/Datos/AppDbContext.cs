using Microsoft.EntityFrameworkCore;
using Entidades.Models;

namespace Datos;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

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
}
}