using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AzureMarketplaceSandbox.Domain.Models;

public class MeteringDimension
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }

    [JsonPropertyName("id")]
    public string DimensionId { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    [JsonPropertyName("pricePerUnit")]
    public decimal PricePerUnit { get; set; }

    [JsonPropertyName("unitOfMeasure")]
    public string UnitOfMeasure { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonIgnore]
    public int PlanId { get; set; }

    [JsonIgnore]
    public Plan Plan { get; set; } = null!;
}
