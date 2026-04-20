using AzureMarketplaceSandbox.Data;
using AzureMarketplaceSandbox.Services;
using Microsoft.EntityFrameworkCore;

namespace AzureMarketplaceSandbox.Api.Middleware;

public class TenantResolutionMiddleware(RequestDelegate next)
{
    private const string EntraOidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public async Task InvokeAsync(
        HttpContext context,
        MarketplaceDbContext db,
        ITenantContext tenantContext,
        TenantBootstrapService bootstrap)
    {
        // API paths set their tenant via SandboxBearerHandler — nothing to do here.
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var oidClaim = context.User.FindFirst(EntraOidClaimType)
                    ?? context.User.FindFirst("oid");
        if (oidClaim is null || !Guid.TryParse(oidClaim.Value, out var oid))
        {
            await next(context);
            return;
        }

        var tenant = await db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.EntraObjectId == oid);

        if (tenant is null)
        {
            tenant = await bootstrap.BootstrapAsync(oid, context.User);
        }
        else
        {
            tenantContext.Set(tenant.Id, tenant.EntraObjectId);
            tenant.LastLoginAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        await next(context);
    }
}
