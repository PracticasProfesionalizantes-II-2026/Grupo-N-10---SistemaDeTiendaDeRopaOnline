using Scalar.AspNetCore;
using Datos;
using Microsoft.EntityFrameworkCore;
using Repositorios.Interfaces;
using Repositorios.Implementaciones;

using Logica.Interfaces;
using Logica.Services;

using Endpoints;

var builder = WebApplication.CreateBuilder(args);

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

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
//======================================
// Endpoints
//======================================

app.MapCategoriaEndpoints();
app.MapSubcategoriaEndpoints();
app.MapEnvioEndpoints();
app.MapPedidoEndpoints();
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
app.Run();
