using Entidades.Enums;
using Entidades.Models;

public class EnvioService : IEnvioService
{
    private readonly IEnvioRepository _repository;

    public EnvioService(IEnvioRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EnvioResponse>> GetByPedidoAsync(int pedidoId)
    {
        var envios = await _repository.GetByPedidoIdAsync(pedidoId);

        return envios.Select(e => new EnvioResponse
        {
            IdEnvio = e.IdEnvio,
            TipoEnvio = e.TipoEnvio.ToString(),
            Costo = e.Costo,
            FechaEnvio = DateTime.UtcNow,
            NumeroSeguimiento = e.NumeroSeguimiento,
            Estado = e.Estado.ToString()
        }).ToList();
    }

    public async Task<EnvioResponse?> GetByIdAsync(int pedidoId, int envioId)
    {
        var envio = await _repository.GetByIdAsync(pedidoId, envioId);

        if (envio == null)
            return null;

        return new EnvioResponse
        {
            IdEnvio = envio.IdEnvio,
            TipoEnvio = envio.TipoEnvio.ToString(),
            Costo = envio.Costo,
            FechaEnvio = DateTime.UtcNow,
            NumeroSeguimiento = envio.NumeroSeguimiento,
            Estado = envio.Estado.ToString()
        };
    }

    public async Task<EnvioResponse> CreateAsync(int pedidoId, CreateEnvioRequest request)
    {
        var envio = new Envio
        {
            PedidoId = pedidoId,
            TipoEnvio = Enum.Parse<TipoEnvio>(request.TipoEnvio, true),
            Costo = request.Costo,
            FechaEnvio = DateTime.UtcNow,
            NumeroSeguimiento = request.NumeroSeguimiento,
            Estado = Enum.Parse<EstadoEnvio>(request.Estado, true)
        };

        await _repository.AddAsync(envio);

        return await GetByIdAsync(pedidoId, envio.IdEnvio)
            ?? throw new InvalidOperationException("No se pudo recuperar el envío creado.");
    }

    public async Task<EnvioResponse?> UpdateAsync(int pedidoId, int envioId, UpdateEnvioRequest request)
    {
        var envio = await _repository.GetByIdAsync(pedidoId, envioId);

        if (envio == null)
            return null;

        envio.TipoEnvio = Enum.Parse<TipoEnvio>(request.TipoEnvio, true);
        envio.Costo = request.Costo;
        envio.FechaEnvio = DateTime.UtcNow;
        envio.NumeroSeguimiento = request.NumeroSeguimiento;
        envio.Estado = Enum.Parse<EstadoEnvio>(request.Estado, true);

        await _repository.UpdateAsync(envio);

        return await GetByIdAsync(pedidoId, envioId);
    }

    public async Task<EnvioResponse?> UpdateEstadoAsync(int pedidoId, int envioId, UpdateEstadoEnvioRequest request)
    {
        var envio = await _repository.GetByIdAsync(pedidoId, envioId);

        if (envio == null)
            return null;

        envio.Estado = Enum.Parse<EstadoEnvio>(request.Estado, true);

        await _repository.UpdateAsync(envio);

        return await GetByIdAsync(pedidoId, envioId);
    }

    public async Task<bool> DeleteAsync(int pedidoId, int envioId)
    {
        var envio = await _repository.GetByIdAsync(pedidoId, envioId);

        if (envio == null)
            return false;

        await _repository.DeleteAsync(envio);

        return true;
    }

    public Task<CalcularEnvioResponse> CalcularAsync(CalcularEnvioRequest request)
    {
        return Task.FromResult(new CalcularEnvioResponse
        {
            Costo = 1500,
            FechaEstimada = DateTime.Now.AddDays(3)
        });
    }

    public async Task<EnvioResponse?> GetTrackingAsync(int envioId)
    {
        var envio = await _repository.GetTrackingAsync(envioId);

        if (envio == null)
            return null;

        return new EnvioResponse
        {
            IdEnvio = envio.IdEnvio,
            TipoEnvio = envio.TipoEnvio.ToString(),
            Costo = envio.Costo,
            FechaEnvio = envio.FechaEnvio,
            NumeroSeguimiento = envio.NumeroSeguimiento,
            Estado = envio.Estado.ToString()
        };
    }
}