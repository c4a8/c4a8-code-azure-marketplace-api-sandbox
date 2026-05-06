using AzureMarketplaceSandbox.Configuration;
using AzureMarketplaceSandbox.Data;
using AzureMarketplaceSandbox.Domain.Enums;
using AzureMarketplaceSandbox.Domain.Models;
using AzureMarketplaceSandbox.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AzureMarketplaceSandbox.Tests.Services;

public class SubscriptionServiceTests
{
    private static (MarketplaceDbContext Db, SubscriptionService Service, IServiceProvider Sp) CreateService()
    {
        var tenantContext = new TenantContext();
        tenantContext.Set(1, Guid.NewGuid());
        var dbName = $"TestDb-{Guid.NewGuid()}";

        var services = new ServiceCollection();
        services.AddSingleton<ITenantContext>(tenantContext);
        services.AddSingleton(new TenantIdAssigningInterceptor(tenantContext));
        services.AddDbContext<MarketplaceDbContext>((sp, opts) =>
        {
            opts.UseInMemoryDatabase(dbName);
            opts.AddInterceptors(sp.GetRequiredService<TenantIdAssigningInterceptor>());
        });
        services.Configure<SandboxOptions>(o =>
        {
            o.ActivationDelaySecondsMin = 0;
            o.ActivationDelaySecondsMax = 0;
        });

        var sp = services.BuildServiceProvider();
        var db = sp.GetRequiredService<MarketplaceDbContext>();
        var service = new SubscriptionService(
            db,
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<IOptions<SandboxOptions>>(),
            tenantContext,
            NullLogger<SubscriptionService>.Instance);
        return (db, service, sp);
    }

    private static Subscription CreateSubscription(
        SaasSubscriptionStatus status = SaasSubscriptionStatus.PendingFulfillmentStart,
        string offerId = "offer1",
        string planId = "silver")
    {
        return new Subscription
        {
            SubscriptionId = Guid.NewGuid(),
            Name = "Test",
            OfferId = offerId,
            PublisherId = "pub1",
            PlanId = planId,
            Quantity = 5,
            SaasSubscriptionStatus = status,
            Beneficiary = new AadInfo { EmailId = "t@t.com" },
            Purchaser = new AadInfo { EmailId = "t@t.com" },
            Term = new SubscriptionTerm { TermUnit = "P1M" }
        };
    }

    [Fact]
    public async Task Activate_FromPending_Succeeds()
    {
        var (db, service, _) = CreateService();
        var offer = new Offer { OfferId = "offer1", PublisherId = "pub1", DisplayName = "O1" };
        offer.Plans.Add(new Plan { PlanId = "silver", DisplayName = "Silver", IsPricePerSeat = true });
        db.Offers.Add(offer);
        var sub = CreateSubscription(SaasSubscriptionStatus.PendingFulfillmentStart);
        db.Subscriptions.Add(sub);
        await db.SaveChangesAsync();

        var (success, _) = await service.ActivateAsync(sub.SubscriptionId, "silver", 10);
        Assert.True(success);

        // Activate returns immediately; the actual state change is applied on a background task.
        var deadline = DateTime.UtcNow.AddSeconds(5);
        Subscription? updated = null;
        while (DateTime.UtcNow < deadline)
        {
            db.ChangeTracker.Clear();
            updated = await db.Subscriptions.FirstOrDefaultAsync(s => s.SubscriptionId == sub.SubscriptionId);
            if (updated?.SaasSubscriptionStatus == SaasSubscriptionStatus.Subscribed)
                break;
            await Task.Delay(50);
        }

        Assert.Equal(SaasSubscriptionStatus.Subscribed, updated!.SaasSubscriptionStatus);
        Assert.Equal(10, updated.Quantity);
    }

    [Theory]
    [InlineData(SaasSubscriptionStatus.Subscribed)]
    [InlineData(SaasSubscriptionStatus.Suspended)]
    [InlineData(SaasSubscriptionStatus.Unsubscribed)]
    public async Task Activate_FromNonPending_Fails(SaasSubscriptionStatus status)
    {
        var (db, service, _) = CreateService();
        var sub = CreateSubscription(status);
        db.Subscriptions.Add(sub);
        await db.SaveChangesAsync();

        var (result, _) = await service.ActivateAsync(sub.SubscriptionId, sub.PlanId, null);
        Assert.False(result);
    }

    [Fact]
    public async Task ChangePlan_ToSamePlan_ReturnsNull()
    {
        var (db, service, _) = CreateService();
        var sub = CreateSubscription(SaasSubscriptionStatus.Subscribed);
        db.Subscriptions.Add(sub);
        var offer = new Offer { OfferId = "offer1", PublisherId = "pub1", DisplayName = "O1" };
        offer.Plans.Add(new Plan { PlanId = "silver", DisplayName = "Silver" });
        db.Offers.Add(offer);
        await db.SaveChangesAsync();

        var op = await service.ChangePlanAsync(sub.SubscriptionId, "silver");
        Assert.Null(op);
    }

    [Fact]
    public async Task ChangePlan_ToNewPlan_CreatesOperation()
    {
        var (db, service, _) = CreateService();
        var sub = CreateSubscription(SaasSubscriptionStatus.Subscribed);
        db.Subscriptions.Add(sub);
        var offer = new Offer { OfferId = "offer1", PublisherId = "pub1", DisplayName = "O1" };
        offer.Plans.Add(new Plan { PlanId = "silver", DisplayName = "Silver" });
        offer.Plans.Add(new Plan { PlanId = "gold", DisplayName = "Gold" });
        db.Offers.Add(offer);
        await db.SaveChangesAsync();

        var op = await service.ChangePlanAsync(sub.SubscriptionId, "gold");

        Assert.NotNull(op);
        Assert.Equal(OperationAction.ChangePlan, op.Action);
        Assert.Equal(OperationStatus.InProgress, op.Status);
        Assert.Equal("gold", op.PlanId);
    }

    [Fact]
    public async Task ChangeQuantity_ZeroOrNegative_ReturnsNull()
    {
        var (db, service, _) = CreateService();
        var sub = CreateSubscription(SaasSubscriptionStatus.Subscribed);
        db.Subscriptions.Add(sub);
        await db.SaveChangesAsync();

        Assert.Null(await service.ChangeQuantityAsync(sub.SubscriptionId, 0));
        Assert.Null(await service.ChangeQuantityAsync(sub.SubscriptionId, -1));
    }

    [Fact]
    public async Task Unsubscribe_AlreadyUnsubscribed_ReturnsNull()
    {
        var (db, service, _) = CreateService();
        var sub = CreateSubscription(SaasSubscriptionStatus.Unsubscribed);
        db.Subscriptions.Add(sub);
        await db.SaveChangesAsync();

        var op = await service.UnsubscribeAsync(sub.SubscriptionId);
        Assert.Null(op);
    }

    [Fact]
    public async Task Unsubscribe_Subscribed_CreatesOperation()
    {
        var (db, service, _) = CreateService();
        var sub = CreateSubscription(SaasSubscriptionStatus.Subscribed);
        db.Subscriptions.Add(sub);
        await db.SaveChangesAsync();

        var op = await service.UnsubscribeAsync(sub.SubscriptionId);

        Assert.NotNull(op);
        Assert.Equal(OperationAction.Unsubscribe, op.Action);
    }

    [Fact]
    public async Task ListAsync_Pagination_Works()
    {
        var (db, service, _) = CreateService();
        for (int i = 0; i < 15; i++)
        {
            var sub = CreateSubscription();
            sub.Name = $"Sub {i}";
            sub.Created = DateTime.UtcNow.AddMinutes(i);
            db.Subscriptions.Add(sub);
        }
        await db.SaveChangesAsync();

        var (page1, nextLink1) = await service.ListAsync(null, 10);
        Assert.Equal(10, page1.Count);
        Assert.NotNull(nextLink1);

        // Extract continuation token
        var token = nextLink1.Split("continuationToken=")[1].Split("&")[0];
        var (page2, nextLink2) = await service.ListAsync(token, 10);
        Assert.Equal(5, page2.Count);
        Assert.Null(nextLink2);
    }
}
