using DTO.Sucursal.Request;
using Datos;
using Microsoft.EntityFrameworkCore;
using Repositorios.Interfaces;

namespace Endpoints;

public static class SucursalEndpoints
{
    public static RouteGroupBuilder MapSucursalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sucursales")
            .WithTags("Sucursales");

        group.MapGet("/", async (ISucursalRepository repo) =>
        {
            return Results.Ok(await repo.GetAllAsync());
        });

        group.MapGet("/{id:int}", async (int id, ISucursalRepository repo) =>
        {
            var sucursal = await repo.GetByIdAsync(id);

            return sucursal is null
                ? Results.NotFound()
                : Results.Ok(sucursal);
        });

        group.MapPost("/", async (CreateSucursalRequest request, ISucursalRepository repo) =>
        {
            var sucursal = await repo.CreateAsync(request);

            return Results.Created($"/api/sucursales/{sucursal.Id}", sucursal);
        });

        group.MapPut("/{id:int}", async (int id, UpdateSucursalRequest request, ISucursalRepository repo) =>
        {
            var actualizado = await repo.UpdateAsync(id, request);

            return actualizado
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, ISucursalRepository repo, AppDbContext db) =>
        {
            var tieneUsuarios = await db.Usuarios.AnyAsync(u => u.SucursalId == id);
            if (tieneUsuarios)
            {
                return Results.Conflict("No se puede eliminar una sucursal con empleados o administradores asignados.");
            }

            var eliminado = await repo.DeleteAsync(id);

            return eliminado
                ? Results.NoContent()
                : Results.NotFound();
        });

        return group;
    }
}