using Entidades.Models;

public class DetallePedidoService : IDetallePedidoService
{
    private readonly IDetallePedidoRepository _repository;

    public DetallePedidoService(IDetallePedidoRepository repository)
    {
        _repository = repository;
    }

    private DetallePedidoResponse Map(DetallePedido d)
    {
        return new DetallePedidoResponse
        {
            IdPedidoDetalle = d.Id,
            PedidoId = d.PedidoId,
            ProductoId = d.ProductoId,
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            Subtotal = d.Subtotal
        };
    }

    public async Task<List<DetallePedidoResponse>> GetByPedidoAsync(int pedidoId)
    {
        var detalles = await _repository.GetByPedidoIdAsync(pedidoId);
        return detalles.Select(Map).ToList();
    }

    public async Task<DetallePedidoResponse?> GetByIdAsync(int pedidoId, int detalleId)
    {
        var detalle = await _repository.GetByIdAsync(pedidoId, detalleId);

        return detalle == null ? null : Map(detalle);
    }

    public async Task<DetallePedidoResponse> CreateAsync(int pedidoId, CreateDetallePedidoRequest request)
    {
        var detalle = new DetallePedido
        {
            PedidoId = pedidoId,
            ProductoId = request.ProductoId,
            Cantidad = request.Cantidad,
            PrecioUnitario = request.PrecioUnitario,
            Subtotal = request.Cantidad * request.PrecioUnitario
        };

        await _repository.AddAsync(detalle);

        return Map(detalle);
    }

    public async Task<DetallePedidoResponse?> UpdateAsync(int pedidoId, int detalleId, UpdateDetallePedidoRequest request)
    {
        var detalle = await _repository.GetByIdAsync(pedidoId, detalleId);

        if (detalle == null) return null;

        detalle.Cantidad = request.Cantidad;
        detalle.PrecioUnitario = request.PrecioUnitario;
        detalle.Subtotal = request.Cantidad * request.PrecioUnitario;

        await _repository.UpdateAsync(detalle);

        return Map(detalle);
    }

    public async Task<bool> DeleteAsync(int pedidoId, int detalleId)
    {
        var detalle = await _repository.GetByIdAsync(pedidoId, detalleId);

        if (detalle == null) return false;

        await _repository.DeleteAsync(detalle);

        return true;
    }
}