using System.ComponentModel.DataAnnotations;
namespace Entidades.Models;

public class MedioContacto
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Medio { get; set; } = string.Empty;
    // WhatsApp, Instagram, Gmail, Facebook, etc.

    [Required]
    public string Valor { get; set; } = string.Empty;
    // Número, URL o correo

    // FK Empresa
    public int EmpresaId { get; set; }

    public Empresa Empresa { get; set; } = null!;
}