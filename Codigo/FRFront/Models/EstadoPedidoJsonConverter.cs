using System.Text.Json;
using System.Text.Json.Serialization;

namespace FRFront.Models;

public sealed class EstadoPedidoJsonConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.Number when reader.TryGetInt32(out var estado) => estado switch
            {
                1 => "Confirmado",
                2 => "En Camino",
                3 => "Entregado",
                4 => "Rechazado",
                5 => "Pagado",
                _ => $"Estado {estado}"
            },
            JsonTokenType.Null => string.Empty,
            _ => throw new JsonException($"El estado del pedido tiene un formato inválido: {reader.TokenType}.")
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
