using AzureMarketplaceSandbox.Data;
using AzureMarketplaceSandbox.Domain.Models;
using AzureMarketplaceSandbox.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AzureMarketplaceSandbox.Tests.Infrastructure;

public class SandboxWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestApiToken = "test-token";
    public const string TestPublisherId = "test-pub";

    private readonly string _dbName = $"TestDb-{Guid.NewGuid()}";
    private int? _defaultTenantId;
    private Guid _defaultEntraObjectId = Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Provide dummy AzureAd config so OIDC middleware does not throw
        builder.UseSetting("AzureAd:TenantId", "00000000-0000-0000-0000-000000000000");
        builder.UseSetting("AzureAd:ClientId", "00000000-0000-0000-0000-000000000000");

        builder.ConfigureServices(services =>
        {
            // Remove all DbContext-related registrations
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<MarketplaceDbContext>)
                         || d.ServiceType == typeof(DbContextOptions)
                         || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                .ToList();
            foreach (var d in descriptors)
                services.Remove(d);

            // Re-register with InMemory provider (still applies interceptors from DI)
            services.AddDbContext<MarketplaceDbContext>((sp, options) =>
            {
                options.UseInMemoryDatabase(_dbName);
                options.AddInterceptors(sp.GetRequiredService<TenantIdAssigningInterceptor>());
            });

            // Override ITenantContext so every request in tests resolves to the default tenant.
            // The bearer handler is swapped for tenant-based lookup in a later commit; until
            // then this keeps API tests passing against the new Global Query Filter.
            services.RemoveAll<ITenantContext>();
            services.AddScoped<ITenantContext>(_ =>
            {
                var ctx = new TenantContext();
                if (_defaultTenantId is int id)
                    ctx.Set(id, _defaultEntraObjectId);
                return ctx;
            });
        });

        builder.UseEnvironment("Development");
    }

    public async Task<Tenant> EnsureDefaultTenantAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        await db.Database.EnsureCreatedAsync();

        var tenant = await db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync();
        if (tenant is null)
        {
            tenant = new Tenant
            {
                EntraObjectId = _defaultEntraObjectId,
                DisplayName = "Test Tenant",
                ApiBearerToken = TestApiToken,
                PublisherId = TestPublisherId,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
            };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }

        _defaultTenantId = tenant.Id;
        _defaultEntraObjectId = tenant.EntraObjectId;
        return tenant;
    }

    public async Task SeedAsync(Func<MarketplaceDbContext, Task> seedAction)
    {
        await EnsureDefaultTenantAsync();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        await seedAction(db);
    }

    public HttpClient CreateAuthenticatedClient()
    {
        EnsureDefaultTenantAsync().GetAwaiter().GetResult();
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TestApiToken);
        return client;
    }

    public async Task<Tenant> CreateTenantAsync(string apiBearerToken, string publisherId = "test-pub")
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        await db.Database.EnsureCreatedAsync();
        var tenant = new Tenant
        {
            EntraObjectId = Guid.NewGuid(),
            DisplayName = $"Tenant-{apiBearerToken}",
            ApiBearerToken = apiBearerToken,
            PublisherId = publisherId,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        return tenant;
    }

    public HttpClient CreateClientForTenant(Tenant tenant)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tenant.ApiBearerToken);
        return client;
    }

    public async Task SeedForTenantAsync(Tenant tenant, Func<MarketplaceDbContext, Task> seedAction)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var tenantCtx = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantCtx.Set(tenant.Id, tenant.EntraObjectId);
        await seedAction(db);
    }
}
