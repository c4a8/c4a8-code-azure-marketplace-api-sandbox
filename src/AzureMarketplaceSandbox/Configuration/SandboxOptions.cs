namespace AzureMarketplaceSandbox.Configuration;

public class SandboxOptions
{
    public const string SectionName = "Sandbox";

    public string WebhookUrl { get; set; } = "https://localhost:7100/api/webhook";
    public string LandingPageUrl { get; set; } = "https://localhost:7100/landing";
    public string BaseUrl { get; set; } = "https://localhost:5050";
}
