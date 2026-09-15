using FRFront.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registrar la configuración global de la tienda (Singleton)
builder.Services.AddSingleton<TiendaConfig>();

// Registrar IHttpClientFactory y el cliente nombrado para la API del Backend
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("BackendApi", client =>
{
    // Reemplaza con la URL base y puerto exacto donde corre tu API Backend (ej. 7001)
    client.BaseAddress = new Uri("https://localhost:7001/"); 
});

// Configuración del servicio de sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Duración de la sesión activa
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Middleware de sesión (DEBE estar colocado antes de UseAuthorization)
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// app.Run() siempre debe ser la última instrucción
app.Run();