namespace DTO.MedioContacto.Response;

public class MedioContactoResponse
{
    public int Id { get; set; }

    public string Medio { get; set; } = string.Empty;

    public string Valor { get; set; } = string.Empty;

    public int EmpresaId { get; set; }
}