using System.Text.Json.Serialization;

namespace AzureMarketplaceSandbox.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SaasSubscriptionStatus
{
    PendingFulfillmentStart,
    Subscribed,
    Suspended,
    Unsubscribed
}
