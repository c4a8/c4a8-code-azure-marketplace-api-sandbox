using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AzureMarketplaceSandbox.Domain.Models;

public partial class SubscriptionTerm
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

    /// <summary>
    /// Calculates the end date by adding the term duration to the given start date.
    /// TermUnit format: P{amount}{unit} where unit is M (months) or Y (years), e.g. P1M, P3M, P1Y, P2Y.
    /// </summary>
    public DateTime CalculateEndDate(DateTime startDate)
    {
        var match = TermUnitRegex().Match(TermUnit);
        if (!match.Success)
            return startDate.AddMonths(1); // fallback

        var amount = int.Parse(match.Groups[1].Value);
        var unit = match.Groups[2].Value.ToUpperInvariant();

        return unit == "Y"
            ? startDate.AddYears(amount)
            : startDate.AddMonths(amount);
    }

    [GeneratedRegex(@"^P(\d+)([MY])$", RegexOptions.IgnoreCase)]
    private static partial Regex TermUnitRegex();
}
