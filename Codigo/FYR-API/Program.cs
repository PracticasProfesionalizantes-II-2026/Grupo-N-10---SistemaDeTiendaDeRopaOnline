using Datos;
using Endpoints;
using Logica.Interfaces;
using Logica.Services;
using Microsoft.EntityFrameworkCore;
using Repositorios.Implementaciones;
using Repositorios.Interfaces;
using Scalar.AspNetCore;
using Modelos = Entidades.Models;
using Entidades.Enums;
using Metricas;
using Prometheus;

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
// Repositorios y Servicios
//======================================
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
builder.Services.AddScoped<ISucursalRepository, SucursalRepository>();
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IMedioContactoRepository, MedioContactoRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddHttpClient("Gemini", client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IGeminiCatalogoService, GeminiCatalogoService>();

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await CatalogoInicialSeeder.SeedAsync(db);

        var usuarios = await db.Usuarios.ToListAsync();
        var usuariosSinHash = usuarios
            .Where(usuario => !usuario.PasswordHash.StartsWith("$2", StringComparison.Ordinal))
            .ToList();
        foreach (var usuario in usuariosSinHash)
        {
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);
        }

        if (usuariosSinHash.Count > 0)
        {
            await db.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "La base de datos no está disponible. La API iniciará sin datos persistidos.");
    }
}

app.UseCors("AllowFrontend");
app.UseHttpMetrics();
app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();

//======================================
// Mapeo de Endpoints
//======================================
app.MapCategoriaEndpoints();
app.MapEmpresaEndpoints();
app.MapSubcategoriaEndpoints();
app.MapEnvioEndpoints();
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
app.MapIaCatalogoEndpoints();

app.MapGet("/", () => "Bienvenido a la API de FYR");
app.MapGet("/api/health", async (AppDbContext db) =>
{
    try
    {
        return await db.Database.CanConnectAsync()
            ? Results.Ok(new { disponible = true })
            : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "No se pudo comprobar la disponibilidad de la base de datos.");
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});

//======================================
// Endpoints de Pedidos
//======================================

// 1. Obtener pedidos ordenados del más reciente al más antiguo
app.MapGet("/api/pedidos", async (AppDbContext db) =>
{
    try
    {
        var pedidos = await db.Pedidos.OrderByDescending(p => p.FechaPedido).ThenByDescending(p => p.Id).ToListAsync();
        return Results.Ok(pedidos);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al consultar pedidos: {ex.Message}");
    }
});

app.MapGet("/api/usuarios/{usuarioId:int}/pedidos", async (int usuarioId, AppDbContext db) =>
{
    var pedidos = await db.Pedidos
        .AsNoTracking()
        .Where(pedido => pedido.UsuarioId == usuarioId)
        .OrderByDescending(pedido => pedido.FechaPedido)
        .ThenByDescending(pedido => pedido.Id)
        .ToListAsync();

    return Results.Ok(pedidos);
});

// Limpieza puntual de ventas creadas sin cliente válido o sin importe.
app.MapDelete("/api/pedidos/invalidos", async (AppDbContext db) =>
{
    var pedidosInvalidos = await db.Pedidos
        .Include(p => p.Usuario)
        .Where(p => p.Total <= 0 ||
                EF.Functions.Like(p.Usuario.Nombre, "%MOSTRADOR%") ||
                EF.Functions.Like(p.Usuario.Apellido, "%MOSTRADOR%"))
        .ToListAsync();

    db.Pedidos.RemoveRange(pedidosInvalidos);
    await db.SaveChangesAsync();

    var clientesHuerfanos = await db.Usuarios
        .Where(u => u.Rol == Rol.Cliente &&
                    (EF.Functions.Like(u.Nombre, "%MOSTRADOR%") ||
                     EF.Functions.Like(u.Apellido, "%MOSTRADOR%")) &&
                    !u.Pedidos.Any())
        .ToListAsync();

    db.Usuarios.RemoveRange(clientesHuerfanos);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        pedidosEliminados = pedidosInvalidos.Count,
        clientesEliminados = clientesHuerfanos.Count
    });
});

// 2. Obtener pedido por ID
app.MapGet("/api/pedidos/{id:int}", async (int id, AppDbContext db) =>
{
    var pedido = await db.Pedidos.FindAsync(id);
    return pedido is not null ? Results.Ok(pedido) : Results.NotFound();
});

// 3. Registrar Venta / Pedido en SQL Server guardando todos los datos del cliente
app.MapPost("/api/pedidos", async (AppDbContext db, PedidoRequest pedidoDto) =>
{
    try
    {
        Modelos.Usuario? usuario = null;
        if (pedidoDto.UsuarioId > 0)
        {
            usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == pedidoDto.UsuarioId);
        }

        string emailBuscado = string.IsNullOrWhiteSpace(pedidoDto.Email) 
            ? $"cliente_{DateTime.Now.Ticks}@fyr.com" 
            : pedidoDto.Email.Trim().ToLower();

        string clienteNombre = string.IsNullOrWhiteSpace(pedidoDto.Cliente)
            ? "María González"
            : pedidoDto.Cliente.Trim();

        // Buscar o crear Usuario
        usuario ??= await db.Usuarios.FirstOrDefaultAsync(u => u.Email == emailBuscado);

        if (usuario == null)
        {
            string[] partes = clienteNombre.Split(' ');
            string nom = partes.Length > 0 ? partes[0] : clienteNombre;
            string ape = partes.Length > 1 ? string.Join(" ", partes.Skip(1)) : "";

            usuario = new Modelos.Usuario
            {
                Nombre = nom,
                Apellido = ape,
                Dni = string.IsNullOrWhiteSpace(pedidoDto.Dni) ? null : pedidoDto.Dni.Trim(),
                Email = emailBuscado,
                Telefono = string.IsNullOrWhiteSpace(pedidoDto.Telefono) ? "Sin registrar" : pedidoDto.Telefono.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                Rol = Rol.Cliente
            };

            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();
        }
        else
        {
            usuario.Nombre = clienteNombre.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? usuario.Nombre;
            usuario.Apellido = string.Join(" ", clienteNombre.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1));
            if (!string.IsNullOrWhiteSpace(pedidoDto.Dni))
            {
                usuario.Dni = pedidoDto.Dni.Trim();
            }

            if (!string.IsNullOrWhiteSpace(pedidoDto.Telefono))
            {
                usuario.Telefono = pedidoDto.Telefono.Trim();
            }

            await db.SaveChangesAsync();
        }

        // Crear Pedido
        var nuevoPedido = new Modelos.Pedido
        {
            UsuarioId = usuario.Id,
            FechaPedido = DateTime.Now,
            Total = pedidoDto.Total,
            Estado = (pedidoDto.MetodoPago?.Contains("cuota", StringComparison.OrdinalIgnoreCase) ?? false)
                ? Entidades.Enums.EstadoPedido.PagoEnCuotas
                : Entidades.Enums.EstadoPedido.Pagado,
            MetodoPago = string.IsNullOrWhiteSpace(pedidoDto.MetodoPago) ? "EFECTIVO" : pedidoDto.MetodoPago,
            DireccionEntrega = string.IsNullOrWhiteSpace(pedidoDto.DireccionEntrega)
                ? $"Cliente: {clienteNombre} - DNI: {pedidoDto.Dni} - Email: {pedidoDto.Email} - Tel: {pedidoDto.Telefono}"
                : pedidoDto.DireccionEntrega,
            NumeroSeguimiento = $"SEG-{Random.Shared.Next(10000, 99999)}"
        };

        db.Pedidos.Add(nuevoPedido);
        await db.SaveChangesAsync();

        // Crear Factura
        var ultimaFactura = await db.Facturas.OrderByDescending(f => f.Id).FirstOrDefaultAsync();
        int nuevoNumeroFactura = (ultimaFactura != null ? ultimaFactura.Numero : 0) + 1;

        var nuevaFactura = new Modelos.Factura
        {
            PedidoId = nuevoPedido.Id,
            Tipo = "B",
            Numero = nuevoNumeroFactura,
            Fecha = DateTime.Now,
            Total = nuevoPedido.Total
        };
        
        db.Facturas.Add(nuevaFactura);
        await db.SaveChangesAsync();

        // Crear detalles
        var producto = await db.Productos.FirstOrDefaultAsync();
        if (pedidoDto.Detalle != null && pedidoDto.Detalle.Any())
        {
            foreach (var item in pedidoDto.Detalle)
            {
                var detalle = new Modelos.DetallePedido
                {
                    PedidoId = nuevoPedido.Id,
                    ProductoId = item.ProductoId > 0 ? item.ProductoId : (producto?.Id ?? 1),
                    Cantidad = item.Cantidad > 0 ? item.Cantidad : 1,
                    PrecioUnitario = item.PrecioUnitario,
                    Subtotal = item.Cantidad * item.PrecioUnitario
                };
                db.DetallesPedido.Add(detalle);
            }
            await db.SaveChangesAsync();
        }

        MetricasTienda.IntentosPedido.WithLabels("confirmado").Inc();
        return Results.Ok(new { mensaje = "Venta guardada correctamente en FYR_DB", id = nuevoPedido.Id });
    }
    catch (Exception ex)
    {
        MetricasTienda.IntentosPedido.WithLabels("error").Inc();
        string errorDetallado = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        Console.WriteLine($"\n[ERROR EXPLICITO SQL SERVER]: {errorDetallado}\n");
        return Results.Problem($"Error en SQL Server: {errorDetallado}");
    }
});

app.MapMetrics();

app.Run();

public sealed class PedidoRequest
{
    public int UsuarioId { get; set; }
    public string DireccionEntrega { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
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