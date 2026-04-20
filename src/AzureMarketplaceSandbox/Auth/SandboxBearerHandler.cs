using System.Security.Claims;
using System.Text.Encodings.Web;
using AzureMarketplaceSandbox.Data;
using AzureMarketplaceSandbox.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AzureMarketplaceSandbox.Auth;

public class SandboxBearerHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "SandboxBearer";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return AuthenticateResult.Fail("Missing Authorization header.");
        }

        var headerValue = authHeader.ToString();
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Authorization header must use Bearer scheme.");
        }

        var token = headerValue["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.Fail("Bearer token is empty.");
        }

        var db = Context.RequestServices.GetRequiredService<MarketplaceDbContext>();
        var tenant = await db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.ApiBearerToken == token);
        if (tenant is null)
        {
            return AuthenticateResult.Fail("Invalid token.");
        }

        var tenantContext = Context.RequestServices.GetRequiredService<ITenantContext>();
        tenantContext.Set(tenant.Id, tenant.EntraObjectId);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, tenant.EntraObjectId.ToString()),
            new Claim(ClaimTypes.Name, tenant.DisplayName),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return AuthenticateResult.Success(ticket);
    }
}
