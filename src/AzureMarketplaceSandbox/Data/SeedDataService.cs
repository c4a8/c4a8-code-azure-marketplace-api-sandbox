namespace AzureMarketplaceSandbox.Data;

// Legacy startup seed. Kept as a no-op bridge until the tenant-aware seed flow
// replaces it; see TenantSeedService in a later commit.
public class SeedDataService(ILogger<SeedDataService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Startup seed skipped — seeding now runs per tenant on first login.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
