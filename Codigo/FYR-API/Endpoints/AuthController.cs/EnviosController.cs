using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Entidades.Models;
using Entidades.Enums;
using Logica.DTOs.Envios;


namespace Api.Controllers;

[ApiController]
[Route("pedidos")]
public class EnviosController : ControllerBase
{
    private readonly AppDbContext _context;

    public EnviosController(AppDbContext context)
    {
        _context = context;
    }

    // =========================
    // POST /pedidos/{idPedido}/envios
    // =========================
    [HttpPost("{idPedido}/envios")]
    public IActionResult Crear(int idPedido, CrearEnvioDto dto)
    {
        var pedido = _context.Pedidos.FirstOrDefault(p => p.Id == idPedido);

        if (pedido == null)
            return NotFound("Pedido no existe");

        var envio = new Envio
        {
            PedidoId = idPedido,
            TipoEnvio = Enum.Parse<TipoEnvio>(dto.TipoEnvio, true),
            Costo = dto.Costo,
            FechaEstimada = dto.FechaEstimada,
            Estado = (EstadoEnvio)dto.IdEstado,
            NumeroSeguimiento = dto.NumeroSeguimiento
        };

        _context.Envios.Add(envio);
        _context.SaveChanges();

        return Created("", envio);
    }

    // =========================
    // GET /pedidos/{idPedido}/envios
    // =========================
    [HttpGet("{idPedido}/envios")]
    public IActionResult GetPorPedido(int idPedido)
    {
        var envios = _context.Envios
            .Where(e => e.PedidoId == idPedido)
            .ToList();

        return Ok(envios);
    }

    // =========================
    // GET /pedidos/{idPedido}/envios/{idEnvio}
    // =========================
    [HttpGet("{idPedido}/envios/{idEnvio}")]
    public IActionResult GetById(int idPedido, int idEnvio)
    {
        var envio = _context.Envios
            .FirstOrDefault(e => e.PedidoId == idPedido && e.Id == idEnvio);

        if (envio == null)
            return NotFound();

        return Ok(envio);
    }

    // =========================
    // PUT /pedidos/{idPedido}/envios/{idEnvio}
    // =========================
    [HttpPut("{idPedido}/envios/{idEnvio}")]
    public IActionResult Update(int idPedido, int idEnvio, ActualizarEnvioDto dto)
    {
        var envio = _context.Envios
            .FirstOrDefault(e => e.PedidoId == idPedido && e.Id == idEnvio);

        if (envio == null)
            return NotFound();

        envio.TipoEnvio = Enum.Parse<TipoEnvio>(dto.TipoEnvio, true);
        envio.Costo = dto.Costo;
        envio.FechaEstimada = dto.FechaEstimada;
        envio.Estado = (EstadoEnvio)dto.IdEstado;
        envio.NumeroSeguimiento = dto.NumeroSeguimiento;

        _context.SaveChanges();

        return Ok(envio);
    }

    // =========================
    // PATCH /pedidos/{idPedido}/envios/{idEnvio}/estado
    // =========================
    [HttpPatch("{idPedido}/envios/{idEnvio}/estado")]
    public IActionResult CambiarEstado(int idPedido, int idEnvio, CambiarEstadoDto dto)
    {
        var envio = _context.Envios
            .FirstOrDefault(e => e.PedidoId == idPedido && e.Id == idEnvio);

        if (envio == null)
            return NotFound();

        envio.Estado = (EstadoEnvio)dto.IdEstado;

        _context.SaveChanges();

        return Ok(new
        {
            envio.Id,
            envio.Estado
        });
    }

    // =========================
    // DELETE /pedidos/{idPedido}/envios/{idEnvio}
    // =========================
    [HttpDelete("{idPedido}/envios/{idEnvio}")]
    public IActionResult Delete(int idPedido, int idEnvio)
    {
        var envio = _context.Envios
            .FirstOrDefault(e => e.PedidoId == idPedido && e.Id == idEnvio);

        if (envio == null)
            return NotFound();

        _context.Envios.Remove(envio);
        _context.SaveChanges();

        return NoContent();
    }

    // =========================
    // POST /envios/calcular (GLOBAL)
    // =========================
    [HttpPost("/envios/calcular")]
    public IActionResult Calcular(CalcularEnvioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CodigoPostal))
            return BadRequest();

        return Ok(new
        {
            costo = 1500,
            fechaEstimada = DateTime.Now.AddDays(3)
        });
    }

    // =========================
    // GET /envios/{idEnvio} (TRACKING GLOBAL)
    // =========================
    [HttpGet("/envios/{idEnvio}")]
    public IActionResult Tracking(int idEnvio)
    {
        var envio = _context.Envios.FirstOrDefault(e => e.Id == idEnvio);

        if (envio == null)
            return NotFound();

        return Ok(new
        {
            envio.Id,
            envio.Estado,
            estado = envio.Estado.ToString(),
            envio.FechaEstimada,
            envio.NumeroSeguimiento
        });
    }
}