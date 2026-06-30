using Scalar.AspNetCore;
using Datos;
using Microsoft.EntityFrameworkCore;
using Repositorios;
using Repositorios.Interfaces;
using Logica.Services;
using Logica.Interfaces;
using DTO;


using Endpoints;
using Repositorios.Implementaciones;


var builder = WebApplication.CreateBuilder(args);

// Conexion SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();

builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ISubcategoriaRepository, SubcategoriaRepository>();

builder.Services.AddScoped<ISubcategoriaService, SubcategoriaService>();
builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();

builder.Services.AddScoped<IEnvioRepository, EnvioRepository>();
builder.Services.AddScoped<IEnvioService, EnvioService>();

builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

builder.Services.AddScoped<IDetallePedidoRepository, DetallePedidoRepository>();
builder.Services.AddScoped<IDetallePedidoService, DetallePedidoService>();
 
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

builder.Services.AddScoped<IReporteRepository, ReporteRepository>();
builder.Services.AddScoped<IReporteService, ReporteService>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IPagoService, PagoService>();

builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<ISucursalRepository, SucursalRepository>();
builder.Services.AddScoped<ISucursalService, SucursalService>();
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<IFacturaService, FacturaService>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IMedioContactoRepository, MedioContactoRepository>();
builder.Services.AddScoped<IMedioContactoService, MedioContactoService>();

// Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

// OpenAPI
app.MapOpenApi();

// Scalar
//app.MapAuthEndpoints();
app.MapScalarApiReference();
app.MapCategoriaEndpoints();
app.MapSubcategoriaEndpoints();
app.MapEmpresaEndpoints();
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

