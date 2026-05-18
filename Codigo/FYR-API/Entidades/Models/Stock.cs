using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades.Models;

public class Stock
{
    [Key]
    public int Id { get; set; }

    public int CantidadDisponible { get; set; }

    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public EstadoStock Estado { get; set; }

    // FK Producto
    public int ProductoId { get; set; }

    public Producto Producto { get; set; } = null!;

    // FK Sucursal
    public int SucursalId { get; set; }

    public Sucursal Sucursal { get; set; } = null!;
}