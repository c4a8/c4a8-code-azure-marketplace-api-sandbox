using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AzureMarketplaceSandbox.Domain.Enums;
using AzureMarketplaceSandbox.Domain.Models;
using AzureMarketplaceSandbox.Tests.Infrastructure;

namespace AzureMarketplaceSandbox.Tests.Api;

public class CrossTenantIsolationTests
{
    private static Subscription BuildSubscription(Guid id, SaasSubscriptionStatus status = SaasSubscriptionStatus.Subscribed) => new()
    {
        SubscriptionId = id,
        Name = "Sub",
        OfferId = "offer1",
        PublisherId = "pub",
        PlanId = "plan1",
        Quantity = 1,
        SaasSubscriptionStatus = status,
        Beneficiary = new AadInfo { EmailId = "b@b.com" },
        Purchaser = new AadInfo { EmailId = "p@p.com" },
        Term = new SubscriptionTerm { TermUnit = "P1M" }
    };

    [Fact]
    public async Task GetSubscription_OwnedByOtherTenant_Returns404()
    {
        using var factory = new SandboxWebApplicationFactory();
        var tenantA = await factory.CreateTenantAsync("token-a");
        var tenantB = await factory.CreateTenantAsync("token-b");
        var subIdA = Guid.NewGuid();

        await factory.SeedForTenantAsync(tenantA, async db =>
        {
            db.Subscriptions.Add(BuildSubscription(subIdA));
            await db.SaveChangesAsync();
        });

        var clientB = factory.CreateClientForTenant(tenantB);
        var response = await clientB.GetAsync($"/api/saas/subscriptions/{subIdA}?api-version=2018-08-31");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListSubscriptions_ReturnsOnlyCurrentTenantData()
    {
        using var factory = new SandboxWebApplicationFactory();
        var tenantA = await factory.CreateTenantAsync("token-a");
        var tenantB = await factory.CreateTenantAsync("token-b");
        var subIdA = Guid.NewGuid();
        var subIdB = Guid.NewGuid();

        await factory.SeedForTenantAsync(tenantA, async db =>
        {
            db.Subscriptions.Add(BuildSubscription(subIdA));
            await db.SaveChangesAsync();
        });
        await factory.SeedForTenantAsync(tenantB, async db =>
        {
            db.Subscriptions.Add(BuildSubscription(subIdB));
            await db.SaveChangesAsync();
        });

        var clientA = factory.CreateClientForTenant(tenantA);
        var body = await clientA.GetFromJsonAsync<JsonElement>("/api/saas/subscriptions?api-version=2018-08-31");
        var ids = body.GetProperty("subscriptions").EnumerateArray()
            .Select(s => s.GetProperty("id").GetGuid())
            .ToList();

        Assert.Contains(subIdA, ids);
        Assert.DoesNotContain(subIdB, ids);
    }

    [Fact]
    public async Task PostUsageEvent_ForOtherTenantsSubscription_ReturnsResourceNotFound()
    {
        using var factory = new SandboxWebApplicationFactory();
        var tenantA = await factory.CreateTenantAsync("token-a");
        var tenantB = await factory.CreateTenantAsync("token-b");
        var subIdA = Guid.NewGuid();

        await factory.SeedForTenantAsync(tenantA, async db =>
        {
            var dimension = new MeteringDimension
            {
                DimensionId = "api-calls",
                DisplayName = "API Calls",
                UnitOfMeasure = "calls",
                PricePerUnit = 0.01m
            };
            var offer = new Offer
            {
                OfferId = "offer1",
                PublisherId = "pub",
                DisplayName = "O1",
                Currency = "EUR",
                MeteringDimensions = [dimension]
            };
            var plan = new Plan { PlanId = "plan1", DisplayName = "Plan 1" };
            offer.Plans.Add(plan);
            db.Offers.Add(offer);
            await db.SaveChangesAsync();
            db.PlanMeteringDimensions.Add(new PlanMeteringDimension
            {
                PlanId = plan.Id,
                MeteringDimensionId = dimension.Id
            });
            db.Subscriptions.Add(BuildSubscription(subIdA));
            await db.SaveChangesAsync();
        });

        var clientB = factory.CreateClientForTenant(tenantB);
        var response = await clientB.PostAsJsonAsync("/api/usageEvent?api-version=2018-08-31", new
        {
            resourceId = subIdA,
            quantity = 5.0,
            dimension = "api-calls",
            effectiveStartTime = DateTime.UtcNow.AddMinutes(-5),
            planId = "plan1"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
