using AzureMarketplaceSandbox.Data;
using AzureMarketplaceSandbox.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureMarketplaceSandbox.Services;

public class TokenService(MarketplaceDbContext db)
{
    public async Task<string> GenerateTokenAsync(Guid subscriptionId)
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');

        db.MarketplaceTokens.Add(new MarketplaceToken
        {
            Token = token,
            SubscriptionId = subscriptionId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        });
        await db.SaveChangesAsync();
        return token;
    }

    public async Task<MarketplaceToken?> ResolveTokenAsync(string token)
    {
        var marketplaceToken = await db.MarketplaceTokens
            .FirstOrDefaultAsync(t => t.Token == token);

        if (marketplaceToken is null)
            return null;

        if (marketplaceToken.ExpiresAt < DateTime.UtcNow)
            return null;

        marketplaceToken.IsResolved = true;
        await db.SaveChangesAsync();
        return marketplaceToken;
    }
}
