using Prometheus;

namespace Metricas;

public static class MetricasTienda
{
    public static readonly Counter IntentosPedido = Metrics.CreateCounter(
        "fyr_order_attempts_total",
        "Cantidad de intentos de creación de pedidos, clasificados por resultado.",
        new CounterConfiguration
        {
            LabelNames = new[] { "resultado" }
        });
}
