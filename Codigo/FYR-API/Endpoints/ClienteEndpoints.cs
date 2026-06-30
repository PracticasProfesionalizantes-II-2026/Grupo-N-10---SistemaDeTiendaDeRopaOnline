public static class ClienteEndpoints
{
    public static void MapClienteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/clientes")
        .WithTags("Clientes")
        .WithGroupName("Clientes");
        

        group.MapGet("/", async (IClienteService service) =>
        {
            return Results.Ok(await service.GetAllAsync());
        });

        group.MapGet("/{id}", async (int id, IClienteService service) =>
        {
            var cliente = await service.GetByIdAsync(id);
            return cliente is null ? Results.NotFound() : Results.Ok(cliente);
        });

        group.MapPost("/", async (CreateClienteRequest request, IClienteService service) =>
        {
            var cliente = await service.CreateAsync(request);
            return Results.Created($"/clientes/{cliente.IdUsuario}", cliente);
        });

        group.MapPut("/{id}", async (int id, UpdateClienteRequest request, IClienteService service) =>
        {
            var cliente = await service.UpdateAsync(id, request);
            return cliente is null ? Results.NotFound() : Results.Ok(cliente);
        });

        group.MapDelete("/{id}", async (int id, IClienteService service) =>
        {
            var eliminado = await service.DeleteAsync(id);
            return eliminado ? Results.NoContent() : Results.NotFound();
        });
    }
}