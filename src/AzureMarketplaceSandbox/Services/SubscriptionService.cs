using AzureMarketplaceSandbox.Data;
using AzureMarketplaceSandbox.Domain.Enums;
using AzureMarketplaceSandbox.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureMarketplaceSandbox.Services;

public class SubscriptionService(MarketplaceDbContext db)
{
    public async Task<Subscription?> GetAsync(Guid subscriptionId)
    {
        return await db.Subscriptions
            .Include(s => s.Beneficiary)
            .Include(s => s.Purchaser)
            .Include(s => s.Term)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);
    }

    public async Task<(List<Subscription> Items, string? NextLink)> ListAsync(string? continuationToken, int pageSize = 10)
    {
        int skip = 0;
        if (continuationToken is not null)
        {
            try { skip = int.Parse(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(continuationToken))); }
            catch { /* invalid token, start from 0 */ }
        }

        var total = await db.Subscriptions.CountAsync();
        var items = await db.Subscriptions
            .Include(s => s.Beneficiary)
            .Include(s => s.Purchaser)
            .Include(s => s.Term)
            .OrderBy(s => s.Created)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        string? nextLink = null;
        if (skip + pageSize < total)
        {
            var nextToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes((skip + pageSize).ToString()));
            nextLink = $"/api/saas/subscriptions?continuationToken={nextToken}&api-version=2018-08-31";
        }

        return (items, nextLink);
    }

    public async Task<List<Plan>> ListAvailablePlansAsync(Guid subscriptionId)
    {
        var subscription = await db.Subscriptions.FindAsync(subscriptionId);
        if (subscription is null)
            return [];

        return await db.Plans
            .Include(p => p.MeteringDimensions)
            .Where(p => p.OfferId == subscription.OfferId)
            .ToListAsync();
    }

    public async Task<bool> ActivateAsync(Guid subscriptionId)
    {
        var subscription = await GetAsync(subscriptionId);
        if (subscription is null)
            return false;

        if (subscription.SaasSubscriptionStatus != SaasSubscriptionStatus.PendingFulfillmentStart)
            return false;

        subscription.SaasSubscriptionStatus = SaasSubscriptionStatus.Subscribed;
        subscription.Term.StartDate = DateTime.UtcNow;
        subscription.Term.EndDate = subscription.Term.TermUnit == "P1Y"
            ? DateTime.UtcNow.AddYears(1)
            : DateTime.UtcNow.AddMonths(1);

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<Operation?> ChangePlanAsync(Guid subscriptionId, string newPlanId)
    {
        var subscription = await GetAsync(subscriptionId);
        if (subscription is null)
            return null;

        if (subscription.SaasSubscriptionStatus != SaasSubscriptionStatus.Subscribed)
            return null;

        if (subscription.PlanId == newPlanId)
            return null;

        var planExists = await db.Plans.AnyAsync(p => p.PlanId == newPlanId && p.OfferId == subscription.OfferId);
        if (!planExists)
            return null;

        var operation = new Operation
        {
            SubscriptionId = subscriptionId,
            OfferId = subscription.OfferId,
            PublisherId = subscription.PublisherId,
            PlanId = newPlanId,
            Quantity = subscription.Quantity,
            Action = OperationAction.ChangePlan,
            Status = OperationStatus.InProgress
        };

        db.Operations.Add(operation);
        await db.SaveChangesAsync();
        return operation;
    }

    public async Task<Operation?> ChangeQuantityAsync(Guid subscriptionId, int newQuantity)
    {
        var subscription = await GetAsync(subscriptionId);
        if (subscription is null)
            return null;

        if (subscription.SaasSubscriptionStatus != SaasSubscriptionStatus.Subscribed)
            return null;

        if (newQuantity <= 0 || subscription.Quantity == newQuantity)
            return null;

        var operation = new Operation
        {
            SubscriptionId = subscriptionId,
            OfferId = subscription.OfferId,
            PublisherId = subscription.PublisherId,
            PlanId = subscription.PlanId,
            Quantity = newQuantity,
            Action = OperationAction.ChangeQuantity,
            Status = OperationStatus.InProgress
        };

        db.Operations.Add(operation);
        await db.SaveChangesAsync();
        return operation;
    }

    public async Task<Operation?> UnsubscribeAsync(Guid subscriptionId)
    {
        var subscription = await GetAsync(subscriptionId);
        if (subscription is null)
            return null;

        if (subscription.SaasSubscriptionStatus == SaasSubscriptionStatus.Unsubscribed)
            return null;

        var operation = new Operation
        {
            SubscriptionId = subscriptionId,
            OfferId = subscription.OfferId,
            PublisherId = subscription.PublisherId,
            PlanId = subscription.PlanId,
            Quantity = subscription.Quantity,
            Action = OperationAction.Unsubscribe,
            Status = OperationStatus.InProgress
        };

        db.Operations.Add(operation);
        await db.SaveChangesAsync();
        return operation;
    }
}
