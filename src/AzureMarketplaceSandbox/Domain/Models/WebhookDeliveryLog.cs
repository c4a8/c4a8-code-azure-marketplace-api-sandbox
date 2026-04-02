using System.ComponentModel.DataAnnotations;
using AzureMarketplaceSandbox.Domain.Enums;

namespace AzureMarketplaceSandbox.Domain.Models;

public class WebhookDeliveryLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SubscriptionId { get; set; }

    public OperationAction Action { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public int? ResponseStatusCode { get; set; }

    public string? ResponseBody { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public bool Success { get; set; }
}
