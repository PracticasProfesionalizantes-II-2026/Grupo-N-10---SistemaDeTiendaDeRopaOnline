using System.ComponentModel.DataAnnotations;
namespace Entidades.Models;

public class Empresa
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string NombreComercial { get; set; } = string.Empty;

    [Required]
    public string LogoUrl { get; set; } = string.Empty;

    [Required]
    public string ColorPrimario { get; set; } = string.Empty;

    public string? ColorSecundario { get; set; }

    [Required]
    public string Tipografia { get; set; } = string.Empty;

    [Required]
    public string Cuit { get; set; } = string.Empty;

    [Required]
    public string RazonSocial { get; set; } = string.Empty;

    [Required]
    public string CondicionIVA { get; set; } = string.Empty;

    [Required]
    public string Direccion { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();

    public ICollection<Sucursal> Sucursales { get; set; } = new List<Sucursal>();

    public ICollection<Proveedor> Proveedores { get; set; } = new List<Proveedor>();

    public ICollection<MedioContacto> MediosContacto { get; set; } = new List<MedioContacto>();
}