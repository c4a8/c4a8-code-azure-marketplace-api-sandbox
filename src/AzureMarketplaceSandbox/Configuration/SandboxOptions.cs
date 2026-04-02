namespace AzureMarketplaceSandbox.Configuration;

public class SandboxOptions
{
    public const string SectionName = "Sandbox";

    public string PublisherId { get; set; } = "contoso";
    public string WebhookUrl { get; set; } = "https://localhost:7100/api/webhook";
    public string LandingPageUrl { get; set; } = "https://localhost:7100/landing";
    public string BaseUrl { get; set; } = "https://localhost:5050";
}

public class AuthOptions
{
    public const string SectionName = "Auth";

    public string? RequiredToken { get; set; }
}

public class SeedDataOptions
{
    public const string SectionName = "SeedData";

    public bool Enabled { get; set; } = true;
}
