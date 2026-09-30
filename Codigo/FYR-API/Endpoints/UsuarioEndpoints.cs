using DTO.Usuario.Request;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Entidades.Models;
using Datos;
using Microsoft.EntityFrameworkCore;

public static class UsuarioEndpoints
{
    public static RouteGroupBuilder MapUsuarioEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/usuarios")
            .WithTags("Usuarios");
            

        group.MapGet("/", async (IUsuarioService service) =>
            Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id}", async (int id, IUsuarioService service) =>
        {
            var usuario = await service.GetByIdAsync(id);
            return usuario is null ? Results.NotFound() : Results.Ok(usuario);
        });

        group.MapGet("/{id}/carrito", async (int id, AppDbContext db) =>
        {
            var carrito = await db.Usuarios.Where(usuario => usuario.Id == id).Select(usuario => usuario.CarritoJson).FirstOrDefaultAsync();
            return carrito == null ? Results.NotFound() : Results.Content(carrito, "application/json");
        });

        group.MapPut("/{id}/carrito", async (int id, CarritoRequest request, AppDbContext db) =>
        {
            var usuario = await db.Usuarios.FirstOrDefaultAsync(item => item.Id == id);
            if (usuario == null)
                return Results.NotFound();

            usuario.CarritoJson = request.Contenido;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapPut("/{id}", async (int id, UpdateUsuarioRequest request, IUsuarioService service) =>
        {
            var usuario = await service.UpdateAsync(id, request);
            return usuario is null ? Results.NotFound() : Results.Ok(usuario);
        });

        group.MapDelete("/{id}", async (int id, IUsuarioService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
        group.MapPost("/", async (CreateUsuarioRequest request, IUsuarioService service) =>
        {
            var usuario = await service.AddAsync(request);
            return Results.Created($"/api/usuarios/{usuario}", usuario);
        });
    
        return group;
    }
}

public record CarritoRequest(string Contenido);