public class RegisterRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public string? Telefono { get; set; }
    public string? Domicilio { get; set; }
    public string? Localidad { get; set; }
    public string? IdiomaPreferido { get; set; }
}