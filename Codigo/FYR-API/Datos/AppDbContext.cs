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

        //==========================
        // DECIMALES
        //==========================

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

        //==========================
        // PRODUCTO
        //==========================

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
            

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Subcategoria)
            .WithMany(s => s.Productos)
            .HasForeignKey(p => p.SubcategoriaId)
            .OnDelete(DeleteBehavior.NoAction);

        //==========================
        // PEDIDO -> FACTURA (1:1)
        //==========================

        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Factura)
            .WithOne(f => f.Pedido)
            .HasForeignKey<Factura>(f => f.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        //==========================
        // PEDIDO -> ENVIO (1:1)
        //==========================

        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Envio)
            .WithOne(e => e.Pedido)
            .HasForeignKey<Envio>(e => e.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        //==========================
        // EMPRESA -> PROVEEDOR
        //==========================

        modelBuilder.Entity<Proveedor>()
            .HasOne(p => p.Empresa)
            .WithMany(e => e.Proveedores)
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.NoAction);
        //Proveedor
        modelBuilder.Entity<Proveedor>()
            .HasOne(p => p.Empresa)
            .WithMany(e => e.Proveedores)
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
        //MedioContacto

        modelBuilder.Entity<MedioContacto>()
            .HasOne(m => m.Empresa)
            .WithMany(e => e.MediosContacto)
            .HasForeignKey(m => m.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
        //Sucursal

        modelBuilder.Entity<Sucursal>()
            .HasOne(s => s.Empresa)
            .WithMany(e => e.Sucursales)
            .HasForeignKey(s => s.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
        //Usuario

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Empresa)
            .WithMany(e => e.Usuarios)
            .HasForeignKey(u => u.EmpresaId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}