using Datos;
using Endpoints;
using Logica.Interfaces;
using Logica.Services;
using Microsoft.EntityFrameworkCore;
using Repositorios.Implementaciones;
using Repositorios.Interfaces;
using Scalar.AspNetCore;
// Entidades del modelo de datos
using Modelos = Entidades.Models;

var builder = WebApplication.CreateBuilder(args);

//======================================
// Configuración de CORS
//======================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

//======================================
// Base de Datos
//======================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

//======================================
// Repositories + Services
//======================================

// Categoria
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

// Subcategoria
builder.Services.AddScoped<ISubcategoriaRepository, SubcategoriaRepository>();
builder.Services.AddScoped<ISubcategoriaService, SubcategoriaService>();

// Empresa
builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();

// Envio
builder.Services.AddScoped<IEnvioRepository, EnvioRepository>();
builder.Services.AddScoped<IEnvioService, EnvioService>();

// Pedido
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

// DetallePedido
builder.Services.AddScoped<IDetallePedidoRepository, DetallePedidoRepository>();
builder.Services.AddScoped<IDetallePedidoService, DetallePedidoService>();

// Cliente
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// Reporte
builder.Services.AddScoped<IReporteRepository, ReporteRepository>();
builder.Services.AddScoped<IReporteService, ReporteService>();

// Usuario
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Pago
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IPagoService, PagoService>();

// Notificacion
builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();

// Auth
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

//======================================
// APIs que trabajan DIRECTAMENTE con Repository
//======================================
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<ISucursalRepository, SucursalRepository>();
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IMedioContactoRepository, MedioContactoRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();

//======================================
// OpenAPI
//======================================
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors("AllowFrontend");

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

//======================================
// Endpoints (MapPedidoEndpoints removido para evitar duplicación)
//======================================
app.MapCategoriaEndpoints();
app.MapEmpresaEndpoints();
app.MapSubcategoriaEndpoints();
app.MapEnvioEndpoints();
// app.MapPedidoEndpoints();
app.MapDetallePedidoEndpoints();
app.MapClienteEndpoints();
app.MapReporteEndpoints();
app.MapUsuarioEndpoints();
app.MapPagoEndpoints();
app.MapNotificacionEndpoints();
app.MapAuthEndpoints();

app.MapProveedorEndpoints();
app.MapSucursalEndpoints();
app.MapFacturaEndpoints();
app.MapProductoEndpoints();
app.MapMedioContactoEndpoints();
app.MapStockEndpoints();

app.MapGet("/", () => "Bienvenido a la API de FYR");

//======================================
// Endpoint Único para Guardar Pedidos / Ventas
//======================================
app.MapPost("/api/pedidos", async (AppDbContext db, PedidoRequest pedidoDto) =>
{
    try
    {
        // 1. Obtener Usuario existente
        var usuario = await db.Usuarios.FirstOrDefaultAsync();
        if (usuario == null)
        {
            return Results.Problem("No se encontró ningún usuario registrado en dbo.Usuarios.");
        }

        // 2. Crear el registro en dbo.Pedidos
        var nuevoPedido = new Modelos.Pedido
        {
            UsuarioId = usuario.Id,
            FechaPedido = DateTime.Now,
            Total = pedidoDto.Total,
            Estado = (Entidades.Enums.EstadoPedido)1,
            MetodoPago = string.IsNullOrWhiteSpace(pedidoDto.TipoEntrega) ? "EFECTIVO" : pedidoDto.TipoEntrega,
            DireccionEntrega = "VENTA MOSTRADOR",
            NumeroSeguimiento = $"SEG-{Random.Shared.Next(10000, 99999)}"
        };

        db.Pedidos.Add(nuevoPedido);
        await db.SaveChangesAsync();

        // 3. Crear los detalles de venta en dbo.DetallesPedido
        var producto = await db.Productos.FirstOrDefaultAsync();
        if (producto != null && pedidoDto.Detalle != null && pedidoDto.Detalle.Any())
        {
            foreach (var item in pedidoDto.Detalle)
            {
                var detalle = new Modelos.DetallePedido
                {
                    PedidoId = nuevoPedido.Id,
                    ProductoId = producto.Id,
                    Cantidad = item.Cantidad > 0 ? item.Cantidad : 1,
                    PrecioUnitario = item.PrecioUnitario,
                    Subtotal = item.Cantidad * item.PrecioUnitario
                };
                db.DetallesPedido.Add(detalle);
            }
            await db.SaveChangesAsync();
        }

        return Results.Ok(new { mensaje = "Venta guardada correctamente en FYR_DB", id = nuevoPedido.Id });
    }
    catch (Exception ex)
    {
        string errorDetallado = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        Console.WriteLine($"\n[ERROR EXPLICITO SQL SERVER]: {errorDetallado}\n");
        return Results.Problem($"Error en SQL Server: {errorDetallado}");
    }
});

app.Run();

//======================================
// Modelos de entrada (DTOs requeridos)
//======================================
public sealed class PedidoRequest
{
    public string Cliente { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string? TipoEntrega { get; set; }
    public List<DetallePedidoRequest> Detalle { get; set; } = new();
}

public sealed class DetallePedidoRequest
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}