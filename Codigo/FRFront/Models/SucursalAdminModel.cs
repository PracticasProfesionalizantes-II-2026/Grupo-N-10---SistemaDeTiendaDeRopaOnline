using System.ComponentModel.DataAnnotations;

namespace FRFront.Models;

public class SucursalAdminModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    public string Direccion { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    public string? Email { get; set; }

    public int EmpresaId { get; set; } = 1;
}

public class SucursalGestionViewModel
{
    public SucursalAdminModel Formulario { get; set; } = new();
    public List<SucursalAdminModel> Sucursales { get; set; } = new();
}
