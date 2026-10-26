using System.Net.Http.Json;
using System.Text.Json;
using DTO.IA;
using DTO.Producto.Requests;
using DTO.Producto.Responses;
using Logica.Interfaces;
using Repositorios.Interfaces;

namespace Logica.Services;

public sealed class GeminiCatalogoService : IGeminiCatalogoService
{
    private readonly HttpClient _httpClient;
    private readonly IProductoRepository _productoRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiCatalogoService> _logger;

    public GeminiCatalogoService(
        IHttpClientFactory httpClientFactory,
        IProductoRepository productoRepository,
        IConfiguration configuration,
        ILogger<GeminiCatalogoService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Gemini");
        _productoRepository = productoRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CatalogoIaResponse> BuscarAsync(
        CatalogoIaRequest request,
        CancellationToken cancellationToken)
    {
        var filtros = await GenerarJsonAsync(
            $"Interpreta esta búsqueda de ropa y devuelve SOLO JSON válido con estas propiedades: " +
            $"texto, genero, color, talle, precioMinimo, precioMaximo, soloOfertas, soloDisponibles. " +
            $"Usa null para filtros no mencionados y booleanos false si no se solicitan. " +
            $"No inventes valores. Consulta: {request.Consulta}",
            cancellationToken);

        var filtrosRequest = JsonSerializer.Deserialize<BuscarProductoRequest>(
            filtros,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new BuscarProductoRequest();

        var resultados = await _productoRepository.SearchAsync(filtrosRequest);
        var mensaje = resultados.Total == 0
            ? "No encontramos prendas que coincidan con tu búsqueda."
            : $"Encontramos {resultados.Total} prenda(s) para tu búsqueda.";

        return new CatalogoIaResponse(request.Consulta, resultados.Productos, mensaje);
    }

    public async Task<OutfitIaResponse> RecomendarOutfitAsync(
        OutfitIaRequest request,
        CancellationToken cancellationToken)
    {
        var productos = await _productoRepository.GetAllAsync();
        var catalogo = productos
            .Where(producto => producto.Activo && producto.Stock > 0)
            .Select(producto => new
            {
                producto.Id,
                producto.Nombre,
                producto.Categoria,
                producto.Genero,
                producto.Colores,
                producto.Precio,
                producto.EsOferta
            });

        var json = JsonSerializer.Serialize(catalogo);
        var seleccion = await GenerarJsonAsync(
            $"Sos un asesor de moda para una tienda de ropa. Con este catálogo y estas preferencias, " +
            $"elige un outfit completo de 2 a 4 prendas que combinen. Devuelve SOLO JSON válido con " +
            $"las propiedades ids (arreglo de enteros) y explicacion (string). Usa únicamente IDs del catálogo. " +
            $"Preferencias del cliente: {request.Preferencias}. Catálogo: {json}",
            cancellationToken);

        var resultado = JsonSerializer.Deserialize<OutfitSeleccion>(
            seleccion,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new OutfitSeleccion([], "No pudimos generar una recomendación.");

        var ids = resultado.Ids.Distinct().ToHashSet();
        var prendas = productos.Where(producto => ids.Contains(producto.Id)).ToList();

        return new OutfitIaResponse(resultado.Explicacion, prendas);
    }

    private async Task<string> GenerarJsonAsync(string prompt, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("La integración con Gemini no está configurada.");

        var model = _configuration["Gemini:Model"] ?? "gemini-flash-latest";
        var payload = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { responseMimeType = "application/json", temperature = 0.2 }
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"v1beta/models/{model}:generateContent");
        request.Headers.Add("x-goog-api-key", apiKey);
        request.Content = JsonContent.Create(payload);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Gemini devolvió {StatusCode}: {Details}", response.StatusCode, details);
            throw new HttpRequestException("Gemini no pudo procesar la solicitud.");
        }

        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken);
        var text = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Gemini devolvió una respuesta vacía.");

        return text;
    }

    private sealed record OutfitSeleccion(List<int> Ids, string Explicacion);

    private sealed record GeminiResponse(List<GeminiCandidate>? Candidates);

    private sealed record GeminiCandidate(GeminiContent? Content);

    private sealed record GeminiContent(List<GeminiPart>? Parts);

    private sealed record GeminiPart(string? Text);
}
