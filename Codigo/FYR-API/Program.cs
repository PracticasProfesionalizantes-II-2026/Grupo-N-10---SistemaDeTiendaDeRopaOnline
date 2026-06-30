using Scalar.AspNetCore;
using Datos;
using Microsoft.EntityFrameworkCore;
using Repositorios;
using Logica.Services;
using Repositorios.Interfaces;
using Repositorios.Implementaciones;
using Logica.Interfaces;




using Endpoints;



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
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<ISucursalRepository, SucursalRepository>();
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<IMedioContactoRepository, MedioContactoRepository>();

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
app.MapProductoEndpoints();
app.MapStockEndpoints();
app.MapGroup("/api/proveedores")
    .MapProveedorEndpoints();

app.MapGroup("/api/usuarios")
    .MapUsuarioEndpoints();

app.MapGroup("/api/pagos")
    .MapPagoEndpoints();
app.MapGroup("/api/facturas")
    .MapFacturaEndpoints();
app.MapGroup("/api/sucursales")
    .MapSucursalEndpoints();
app.MapGroup("/api/facturas")
    .MapFacturaEndpoints();
app.MapGroup("/api/medioscontacto")
    .MapMedioContactoEndpoints();
app.Run();