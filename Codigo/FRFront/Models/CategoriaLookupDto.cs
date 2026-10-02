namespace FRFront.Models;

public sealed class CategoriaLookupDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
}
