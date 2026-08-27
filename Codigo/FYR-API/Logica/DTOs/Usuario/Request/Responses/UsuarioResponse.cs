namespace DTO.Usuario.Response;

public class UsuarioResponse
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Rol Rol { get; set; }

    public string? Telefono { get; set; }

    public string? IdiomaPreferido { get; set; }

    public string? FotoPerfil { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? EmpresaId { get; set; }
}