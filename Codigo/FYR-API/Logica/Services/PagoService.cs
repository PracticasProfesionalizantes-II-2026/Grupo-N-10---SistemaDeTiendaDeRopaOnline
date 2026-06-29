using Entidades.Models;

public class PagoService : IPagoService
{
    private readonly IPagoRepository _repository;

    public PagoService(IPagoRepository repository)
    {
        _repository = repository;
    }

    private PagoResponse Map(Pago pago)
    {
        return new PagoResponse
        {
            IdPago = pago.Id,
            Monto = pago.Monto,
            MetodoPago = pago.MetodoPago,
            FechaPago = pago.FechaPago,
            Estado = pago.Estado,
            PedidoId = pago.PedidoId
        };
    }

    public async Task<List<PagoResponse>> GetByPedidoAsync(int pedidoId)
    {
        var pagos = await _repository.GetByPedidoIdAsync(pedidoId);
        return pagos.Select(Map).ToList();
    }

    public async Task<PagoResponse?> GetByIdAsync(int pedidoId, int pagoId)
    {
        var pago = await _repository.GetByIdAsync(pedidoId, pagoId);

        return pago == null ? null : Map(pago);
    }

    public async Task<PagoResponse> CreateAsync(int pedidoId, CreatePagoRequest request)
    {
        var pago = new Pago
        {
            Monto = request.Monto,
            MetodoPago = request.MetodoPago,
            FechaPago = request.FechaPago,
            Estado = request.Estado,
            PedidoId = pedidoId
        };

        await _repository.AddAsync(pago);

        return Map(pago);
    }

    public async Task<PagoResponse?> UpdateAsync(int pedidoId, int pagoId, UpdatePagoRequest request)
    {
        var pago = await _repository.GetByIdAsync(pedidoId, pagoId);

        if (pago == null)
            return null;

        pago.MetodoPago = request.MetodoPago;
        pago.Estado = request.Estado;

        await _repository.UpdateAsync(pago);

        return Map(pago);
    }

    public async Task<bool> DeleteAsync(int pedidoId, int pagoId)
    {
        var pago = await _repository.GetByIdAsync(pedidoId, pagoId);

        if (pago == null)
            return false;

        await _repository.DeleteAsync(pago);

        return true;
    }
}