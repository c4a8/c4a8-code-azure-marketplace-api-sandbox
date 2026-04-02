using System.Text.Json.Serialization;

namespace AzureMarketplaceSandbox.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UsageEventStatus
{
    Accepted,
    Expired,
    Duplicate,
    Error,
    ResourceNotFound,
    ResourceNotAuthorized,
    ResourceNotActive,
    InvalidDimension,
    InvalidQuantity,
    BadArgument
}
