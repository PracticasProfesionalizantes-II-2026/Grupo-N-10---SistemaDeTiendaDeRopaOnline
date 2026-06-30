public static class UsuarioEndpoints
{
    public static RouteGroupBuilder MapUsuarioEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/usuarios")
          .WithTags("Usuarios")
          .WithGroupName("Usuarios");

        group.MapGet("/", async (IUsuarioService service) =>
            Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id}", async (int id, IUsuarioService service) =>
        {
            var usuario = await service.GetByIdAsync(id);
            return usuario is null ? Results.NotFound() : Results.Ok(usuario);
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
        return group;
    }
}