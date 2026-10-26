# Agente Desarrollador FYR y Skill de Catálogo

## Idea implementada

El proyecto incorpora un agente personalizado y una Skill para agilizar el desarrollo de funcionalidades del catálogo y de la API de productos.

- **Agente:** `Desarrollador FYR`
- **Skill:** `Asistente de Catálogo FYR`
- **Ubicación:** `.github/agents/` y `.github/skills/`

La solución es específica del proyecto: conoce la separación entre endpoints, DTOs y repositorios, el uso de Minimal APIs y las entidades de productos, stock, ofertas, categorías, talles y colores.

## Problema que resuelve

Antes de agregar una funcionalidad de catálogo, el desarrollador debía recordar y revisar manualmente:

1. Dónde declarar el endpoint.
2. Qué DTO crear.
3. Cómo acceder a los datos mediante el repositorio.
4. Cómo validar parámetros.
5. Cómo mantener la respuesta compatible con el frontend.
6. Cómo documentar y probar la API.

El agente y la Skill convierten esos pasos en un flujo repetible y con reglas del proyecto.

## Cómo utilizarlo

En VS Code se puede seleccionar el agente **Desarrollador FYR** o invocar la Skill **Asistente de Catálogo FYR**. Un pedido de ejemplo es:

> Agregar un filtro de catálogo para buscar productos por texto, categoría, género, color, talle, precio y disponibilidad. Mantener la arquitectura actual, validar los parámetros y agregar un ejemplo HTTP.

El flujo esperado es:

1. Inspeccionar entidad, DTOs, endpoint, interfaz y repositorio.
2. Diseñar el contrato de la consulta.
3. Implementar la validación en el endpoint.
4. Implementar filtros, orden y paginación en el repositorio.
5. Agregar un ejemplo HTTP.
6. Compilar la API y comunicar el resultado.

## Demostración implementada

Se agregó:

```http
GET /api/productos/buscar
```

### Parámetros disponibles

| Parámetro | Tipo | Descripción |
|---|---|---|
| `texto` | texto | Busca en nombre, marca, descripción, colores, talles y género. |
| `categoriaId` | entero | Filtra por categoría. |
| `subcategoriaId` | entero | Filtra por subcategoría. |
| `genero` | texto | Filtra por género. |
| `color` | texto | Busca dentro de los colores del producto. |
| `talle` | texto | Busca dentro de los talles del producto. |
| `precioMinimo` | decimal | Precio mínimo permitido. |
| `precioMaximo` | decimal | Precio máximo permitido. |
| `soloOfertas` | booleano | Devuelve únicamente ofertas cuando es `true`. |
| `soloDisponibles` | booleano | Devuelve productos con stock cuando es `true`. |
| `incluirInactivos` | booleano | Permite incluir productos inactivos para búsquedas administrativas. |
| `pagina` | entero | Página solicitada; comienza en `1`. |
| `tamanoPagina` | entero | Cantidad de resultados, entre `1` y `100`. |

Ejemplo:

```http
GET /api/productos/buscar?texto=remera&genero=mujer&color=negro&precioMaximo=50000&soloDisponibles=true&pagina=1&tamanoPagina=10
```

La respuesta incluye `productos`, `total`, `pagina`, `tamanoPagina` y `totalPaginas`.

## Validaciones

La API devuelve `400 Bad Request` cuando:

- La página es menor que `1`.
- El tamaño de página no está entre `1` y `100`.
- Un precio es negativo.
- El precio mínimo supera al precio máximo.

Las búsquedas muestran productos activos por defecto y tienen orden determinista por nombre e identificador antes de aplicar la paginación.

## Archivos principales

- `.github/agents/desarrollador-fyr.agent.md`
- `.github/skills/asistente-catalogo-fyr/SKILL.md`
- `Codigo/FYR-API/Logica/DTOs/Producto/Requests/BuscarProductoRequest.cs`
- `Codigo/FYR-API/Logica/DTOs/Producto/Responses/BuscarProductoResponse.cs`
- `Codigo/FYR-API/Endpoints/ProductoEndpoints.cs`
- `Codigo/FYR-API/Repositorios/Interfaces/IProductoRepository.cs`
- `Codigo/FYR-API/Repositorios/Implementaciones/ProductoRepository.cs`
- `Codigo/FYR-API/FYR-API.http`

## Incorporación de IA con Gemini

La aplicación incorpora Gemini mediante la API de Google para dos funcionalidades:

1. **Búsqueda en lenguaje natural:** el cliente puede escribir “busco una campera negra de mujer, talle M, menor a $150.000 y con stock”. Gemini convierte la consulta en filtros estructurados y la API reutiliza la búsqueda avanzada.
2. **Recomendación de outfit:** el cliente describe la ocasión, estilo, género, colores o presupuesto. Gemini selecciona entre los productos activos y disponibles del catálogo y devuelve un conjunto de 2 a 4 prendas con una explicación.

Endpoints:

```http
POST /api/ia/catalogo/buscar
{ "consulta": "campera negra de mujer, talle M y con stock" }

POST /api/ia/catalogo/outfit
{ "preferencias": "outfit casual para una salida de tarde en tonos neutros" }
```

La interfaz del catálogo incluye ambos formularios para que la funcionalidad sea visible para el cliente.

### Configuración segura

La clave no se guarda en Git. Configurarla localmente con User Secrets o una variable de entorno:

```powershell
dotnet user-secrets set "Gemini:ApiKey" "TU_NUEVA_CLAVE"
```

También se puede usar `Gemini__ApiKey` como variable de entorno. El modelo configurable por defecto es `gemini-flash-latest` y la clave se envía mediante el encabezado `x-goog-api-key`.

La disponibilidad y los límites dependen de la cuota vigente de Google AI Studio; la aplicación registra el error y muestra un mensaje controlado si la clave falta, se supera la cuota o Gemini no responde.

> **Seguridad:** nunca subir una clave a Git, `appsettings.json`, capturas ni documentación. La clave compartida durante una conversación debe revocarse y reemplazarse inmediatamente.
