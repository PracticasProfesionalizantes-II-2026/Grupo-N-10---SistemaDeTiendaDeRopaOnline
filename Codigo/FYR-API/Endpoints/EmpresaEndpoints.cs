using Logica.DTO.Empresa.Request;
using Logica.Interfaces;

namespace Endpoints;

public static class EmpresaEndpoints
{
    public static void MapEmpresaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/empresa")
                       .WithTags("Empresa");

        // Obtener todas las empresas
        group.MapGet("/", async (IEmpresaService service) =>
        {
            var empresas = await service.GetAllAsync();
            return Results.Ok(empresas);
        });

        // Obtener empresa por Id
        group.MapGet("/{id:int}", async (int id, IEmpresaService service) =>
        {
            var empresa = await service.GetByIdAsync(id);

            return empresa is null
                ? Results.NotFound()
                : Results.Ok(empresa);
        });

        // Obtener empresa por CUIT
        group.MapGet("/cuit/{cuit}", async (string cuit, IEmpresaService service) =>
        {
            try
            {
                var empresa = await service.GetByCuitAsync(cuit);
                return Results.Ok(empresa);
            }
            catch (Exception ex)
            {
                return Results.NotFound(ex.Message);
            }
        });

        // Crear empresa
        group.MapPost("/", async (CrearEmpresaRequest request, IEmpresaService service) =>
        {
            try
            {
                var empresa = await service.CrearAsync(request);

                return Results.Created($"/empresa/{empresa.Id}", empresa);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        // Actualizar empresa
        group.MapPut("/{id:int}", async (int id, ActualizarEmpresaRequest request, IEmpresaService service) =>
        {
            var actualizado = await service.ActualizarAsync(id, request);

            return actualizado
                ? Results.NoContent()
                : Results.NotFound();
        });

        // Eliminar empresa
        group.MapDelete("/{id:int}", async (int id, IEmpresaService service) =>
        {
            var eliminado = await service.EliminarAsync(id);

            return eliminado
                ? Results.NoContent()
                : Results.NotFound();
        });
    }
}