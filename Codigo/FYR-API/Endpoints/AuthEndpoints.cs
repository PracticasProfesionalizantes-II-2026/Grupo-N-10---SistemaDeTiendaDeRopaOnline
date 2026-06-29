public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthService service) =>
        {
            var user = await service.RegisterAsync(request);
            return Results.Created($"/usuarios/{user.IdUsuario}", user);
        });

        group.MapPost("/login", async (LoginRequest request, IAuthService service) =>
        {
            var result = await service.LoginAsync(request);

            return result == null
                ? Results.Unauthorized()
                : Results.Ok(result);
        });

        group.MapPost("/forgot-password", async (ForgotPasswordRequest request, IAuthService service) =>
        {
            var token = await service.ForgotPasswordAsync(request);

            return token == null
                ? Results.NotFound()
                : Results.Ok(new { mensaje = "Token generado", token });
        });

        group.MapPost("/reset-password", async (ResetPasswordRequest request, IAuthService service) =>
        {
            var ok = await service.ResetPasswordAsync(request);

            return ok
                ? Results.Ok(new { mensaje = "Contraseña actualizada correctamente" })
                : Results.BadRequest();
        });
    }
}