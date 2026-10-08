# Monitoreo con Prometheus y Grafana

## Métricas implementadas

La API expone `http://localhost:5097/metrics`.

1. **Disponibilidad de la API**
   Prometheus.NET registra las solicitudes HTTP. La tasa de errores puede calcularse con:

   ```promql
   100 * (
     1 - sum(rate(http_requests_received_total{code=~"5.."}[5m]))
       / sum(rate(http_requests_received_total[5m]))
   )
   ```

2. **Latencia de endpoints críticos**
   Prometheus.NET registra la duración de las solicitudes en segundos:

   ```promql
   histogram_quantile(
     0.95,
   sum by (le, endpoint) (
     rate(http_request_duration_seconds_bucket{endpoint=~"/api/(productos|auth/login|pedidos).*"}[5m])
     )
   )
   ```

3. **Tasa de pedidos confirmados**
   El endpoint real de creación de pedidos incrementa
   `fyr_order_attempts_total{resultado="confirmado"}` o
   `fyr_order_attempts_total{resultado="error"}`:

   ```promql
   100 * sum(rate(fyr_order_attempts_total{resultado="confirmado"}[15m]))
     / sum(rate(fyr_order_attempts_total[15m]))
   ```

## Ejecutar

1. Iniciar la API con el perfil HTTP:

   ```powershell
   dotnet run --project .\Codigo\FYR-API\FYR-API.csproj --launch-profile http
   ```

2. Verificar las métricas:

   ```text
   http://localhost:5097/metrics
   ```

3. Iniciar Prometheus y Grafana:

   ```powershell
   docker compose -f .\monitoring\docker-compose.yml up -d
   ```

4. Abrir:
   - Prometheus: `http://localhost:9090`
   - Grafana: `http://localhost:3000`

Grafana queda configurado automáticamente con la fuente de datos Prometheus y
el dashboard `FYR - Monitoreo de la tienda`. La URL interna de la fuente es
`http://prometheus:9090`.

La contraseña inicial de Grafana para este entorno local es `fyr-admin`; debe
cambiarse antes de utilizarlo fuera de desarrollo.
