using Entidades.Enums;
using Entidades.Models;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _repository;

    public PedidoService(IPedidoRepository repository)
    {
        _repository = repository;
    }

    private PedidoResponse Map(Pedido p)
    {
        return new PedidoResponse
        {
            IdPedido = p.Id,
            FechaPedido = p.FechaPedido,
            Total = p.Total,
            DireccionEntrega = p.DireccionEntrega,
            MetodoPago = p.MetodoPago,
            NumeroSeguimiento = p.NumeroSeguimiento,
            UsuarioId = p.UsuarioId,
            Estado = p.Estado.ToString()
        };
    }

    public async Task<List<PedidoResponse>> GetAllAsync()
    {
        var pedidos = await _repository.GetAllAsync();
        return pedidos.Select(Map).ToList();
    }

    public async Task<PedidoResponse?> GetByIdAsync(int id)
    {
        var pedido = await _repository.GetByIdAsync(id);
        return pedido == null ? null : Map(pedido);
    }

    public async Task<List<PedidoResponse>> GetByUsuarioAsync(int usuarioId)
    {
        var pedidos = await _repository.GetByUsuarioIdAsync(usuarioId);
        return pedidos.Select(Map).ToList();
    }

    public async Task<PedidoResponse> CreateAsync(CreatePedidoRequest request)
    {
        var pedido = new Pedido
        {
            FechaPedido = request.FechaPedido,
            DireccionEntrega = request.DireccionEntrega,
            MetodoPago = request.MetodoPago,
            UsuarioId = request.UsuarioId,
            Estado = Enum.Parse<EstadoPedido>(request.Estado, true)
        };

        await _repository.AddAsync(pedido);

        return Map(pedido);
    }

    public async Task<PedidoResponse?> UpdateAsync(int id, UpdatePedidoRequest request)
    {
        var pedido = await _repository.GetByIdAsync(id);

        if (pedido == null) return null;

        pedido.DireccionEntrega = request.DireccionEntrega;
        pedido.MetodoPago = request.MetodoPago;

        await _repository.UpdateAsync(pedido);

        return Map(pedido);
    }

    public async Task<PedidoResponse?> UpdateEstadoAsync(int id, UpdateEstadoPedidoRequest request)
    {
        var pedido = await _repository.GetByIdAsync(id);

        if (pedido == null) return null;

        pedido.Estado = Enum.Parse<EstadoPedido>(request.Estado, true);

        await _repository.UpdateAsync(pedido);

        return Map(pedido);
    }

    public async Task<PedidoResponse?> UpdateSeguimientoAsync(int id, UpdateSeguimientoPedidoRequest request)
    {
        var pedido = await _repository.GetByIdAsync(id);

        if (pedido == null) return null;

        pedido.NumeroSeguimiento = request.NumeroSeguimiento;

        await _repository.UpdateAsync(pedido);

        return Map(pedido);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var pedido = await _repository.GetByIdAsync(id);

        if (pedido == null) return false;

        await _repository.DeleteAsync(pedido);

        return true;
    }
}