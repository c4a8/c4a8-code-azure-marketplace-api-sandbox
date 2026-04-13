using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureMarketplaceSandbox.Domain.Models;

public class PlanComponents
{
    [JsonPropertyName("recurrentBillingTerms")]
    public List<RecurrentBillingTerm> RecurrentBillingTerms { get; set; } = [];

    [JsonPropertyName("meteringDimensions")]
    public List<MeteringDimension> MeteringDimensions { get; set; } = [];
}

public class RecurrentBillingTerm
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("termUnit")]
    public string TermUnit { get; set; } = string.Empty;

    [JsonPropertyName("termDescription")]
    public string TermDescription { get; set; } = string.Empty;

    [JsonPropertyName("meteredQuantityIncluded")]
    public List<MeteredQuantityIncluded> MeteredQuantityIncluded { get; set; } = [];
}

public class MeteredQuantityIncluded
{
    [JsonPropertyName("dimensionId")]
    public string DimensionId { get; set; } = string.Empty;

    [JsonPropertyName("units")]
    [JsonConverter(typeof(DecimalAsStringConverter))]
    public decimal Units { get; set; }
}

public class DecimalAsStringConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.String
            ? decimal.Parse(reader.GetString()!, CultureInfo.InvariantCulture)
            : reader.GetDecimal();

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
}
