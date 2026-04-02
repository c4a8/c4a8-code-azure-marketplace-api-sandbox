using System.Text.Json.Serialization;

namespace AzureMarketplaceSandbox.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationStatus
{
    NotStarted,
    InProgress,
    Failed,
    Succeeded,
    Conflict
}
