using System.Text.Json.Serialization;

namespace AzureMarketplaceSandbox.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationAction
{
    ChangePlan,
    ChangeQuantity,
    Reinstate,
    Suspend,
    Unsubscribe,
    Renew
}
