using AzureMarketplaceSandbox.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AzureMarketplaceSandbox.Data;

public class TenantIdAssigningInterceptor(ITenantContext tenantContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AssignTenantIds(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AssignTenantIds(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AssignTenantIds(DbContextEventData eventData)
    {
        if (tenantContext.TenantId is not int tenantId) return;
        if (eventData.Context is null) return;

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added) continue;
            if (entry.Metadata.FindProperty("TenantId") is null) continue;

            var prop = entry.Property("TenantId");
            if (prop.CurrentValue is int current && current == 0)
                prop.CurrentValue = tenantId;
        }
    }
}
