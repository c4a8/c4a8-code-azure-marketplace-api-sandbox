using System.Text.Json.Serialization;
using AzureMarketplaceSandbox.Domain.Enums;

namespace AzureMarketplaceSandbox.Domain.Models;

/// <summary>
/// Outgoing webhook payload matching Microsoft's documented shape.
/// Not persisted in the database — used for serialization only.
/// </summary>
public class WebhookPayload
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("activityId")]
    public Guid ActivityId { get; set; } = Guid.NewGuid();

    [JsonPropertyName("publisherId")]
    public string PublisherId { get; set; } = string.Empty;

    [JsonPropertyName("offerId")]
    public string OfferId { get; set; } = string.Empty;

    [JsonPropertyName("planId")]
    public string PlanId { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int? Quantity { get; set; }

    [JsonPropertyName("subscriptionId")]
    public Guid SubscriptionId { get; set; }

    [JsonPropertyName("timeStamp")]
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("action")]
    public OperationAction Action { get; set; }

    [JsonPropertyName("status")]
    public OperationStatus Status { get; set; } = OperationStatus.InProgress;

    [JsonPropertyName("operationRequestSource")]
    public string OperationRequestSource { get; set; } = "Azure";

    [JsonPropertyName("subscription")]
    public Subscription Subscription { get; set; } = null!;

    [JsonPropertyName("purchaseToken")]
    public string? PurchaseToken { get; set; }
}
