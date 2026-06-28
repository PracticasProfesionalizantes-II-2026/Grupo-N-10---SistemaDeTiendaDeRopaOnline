using Scalar.AspNetCore;
using Datos;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Interfaces;

using Logica.Services;
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

builder.Services.AddScoped<IEnvioRepository, EnvioRepository>();
builder.Services.AddScoped<IEnvioService, EnvioService>();

builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

builder.Services.AddScoped<IDetallePedidoRepository, DetallePedidoRepository>();
builder.Services.AddScoped<IDetallePedidoService, DetallePedidoService>();
 

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

app.Run();

//Hola Mundo