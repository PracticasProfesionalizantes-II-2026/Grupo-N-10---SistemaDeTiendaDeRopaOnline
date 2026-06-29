using DTO.Usuario.Request;
using DTO.Usuario.Response;
namespace DTO.Usuario.Responses;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }

    public UsuarioResponse Usuario { get; set; } = new();
}