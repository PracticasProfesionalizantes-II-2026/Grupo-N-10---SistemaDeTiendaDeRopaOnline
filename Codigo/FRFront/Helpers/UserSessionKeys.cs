using Microsoft.AspNetCore.Http;

namespace FRFront.Helpers;

public static class UserSessionKeys
{
    public static string ForUser(ISession session, string key)
    {
        var usuario = session.GetString("UsuarioSesion");
        return string.IsNullOrWhiteSpace(usuario)
            ? key
            : $"{key}:{usuario.Trim().ToLowerInvariant()}";
    }
}
