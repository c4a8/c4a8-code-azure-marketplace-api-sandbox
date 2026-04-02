using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AzureMarketplaceSandbox.Domain.Models;

public class SubscriptionTerm
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }

    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }

    [JsonPropertyName("termUnit")]
    public string TermUnit { get; set; } = "P1M";
}
