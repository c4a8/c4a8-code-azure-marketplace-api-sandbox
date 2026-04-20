using AzureMarketplaceSandbox.Data;
using AzureMarketplaceSandbox.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;

namespace AzureMarketplaceSandbox.Auth;

// Blazor circuits run in a DI scope separate from the initial HTTP request, so
// TenantResolutionMiddleware cannot populate their ITenantContext. This handler
// resolves the tenant from the authenticated user once when a circuit opens.
public class TenantCircuitHandler(
    ITenantContext tenantContext,
    MarketplaceDbContext db,
    AuthenticationStateProvider authStateProvider,
    ILogger<TenantCircuitHandler> logger) : CircuitHandler
{
    private const string EntraOidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var state = await authStateProvider.GetAuthenticationStateAsync();
        var user = state.User;
        if (user.Identity?.IsAuthenticated != true) return;

        var oidClaim = user.FindFirst(EntraOidClaimType) ?? user.FindFirst("oid");
        if (oidClaim is null || !Guid.TryParse(oidClaim.Value, out var oid)) return;

        var tenant = await db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.EntraObjectId == oid, cancellationToken);
        if (tenant is null)
        {
            logger.LogWarning("Circuit opened for oid {Oid} but no tenant row exists.", oid);
            return;
        }

        tenantContext.Set(tenant.Id, tenant.EntraObjectId);
    }
}
