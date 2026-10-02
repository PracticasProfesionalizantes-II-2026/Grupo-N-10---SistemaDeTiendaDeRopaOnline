using Entidades.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _repository;
    private readonly IConfiguration _configuration;

    private static Dictionary<string, int> ResetTokens = new();

    public AuthService(IAuthRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<UsuarioResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await _repository.GetByEmailAsync(email);

        if (existing != null)
            throw new InvalidOperationException("El usuario ya existe");

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Rol = Rol.Cliente,
            Telefono = request.Telefono,
            IdiomaPreferido = request.IdiomaPreferido
        };

        await _repository.AddAsync(usuario);

        return new UsuarioResponse
        {
            IdUsuario = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            TipoUsuario = usuario.Rol.ToString()
        };
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var usuario = await _repository.GetByEmailAsync(request.Email);

        if (usuario == null || !usuario.Activo)
            return null;

        bool validPassword;
        try
        {
            validPassword = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
        }
        catch (ArgumentException)
        {
            // Compatibilidad única con cuentas antiguas que todavía tenían la clave sin hash.
            validPassword = string.Equals(request.Password, usuario.PasswordHash, StringComparison.Ordinal);
            if (validPassword)
            {
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                await _repository.UpdateAsync(usuario);
            }
        }

        if (!validPassword)
            return null;

        var token = GenerateJwt(usuario);

        return new LoginResponse
        {
            Token = token,
            IdUsuario = usuario.Id,
            TipoUsuario = usuario.Rol.ToString()
        };
    }

    public async Task<string?> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var usuario = await _repository.GetByEmailAsync(request.Email);

        if (usuario == null)
            return null;

        var token = Guid.NewGuid().ToString();

        ResetTokens[token] = usuario.Id;

        return token;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (!ResetTokens.ContainsKey(request.Token))
            return false;

        var userId = ResetTokens[request.Token];

        var usuario = await _repository.GetByIdAsync(userId);

        if (usuario == null)
            return false;

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _repository.UpdateAsync(usuario);

        ResetTokens.Remove(request.Token);

        return true;
    }

    private string GenerateJwt(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}