public class ClienteResponse
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? IdiomaPreferido { get; set; }
    public bool Activo { get; set; }
    public string? FotoPerfil { get; set; }
}