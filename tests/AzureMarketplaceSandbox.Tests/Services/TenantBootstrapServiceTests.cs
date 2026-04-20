using AzureMarketplaceSandbox.Services;

namespace AzureMarketplaceSandbox.Tests.Services;

public class TenantBootstrapServiceTests
{
    [Theory]
    [InlineData("peter.meier@contoso.com", "peter-meier")]
    [InlineData("Peter.Meier@contoso.com", "peter-meier")]
    [InlineData("first.middle.last@corp.example", "first-middle-last")]
    [InlineData("single@corp.com", "single")]
    [InlineData(null, "my-publisher")]
    [InlineData("", "my-publisher")]
    [InlineData("   ", "my-publisher")]
    public void DerivePublisherId_UsesUpnUserPart(string? upn, string expected)
    {
        Assert.Equal(expected, TenantBootstrapService.DerivePublisherId(upn));
    }

    [Fact]
    public void GenerateBearerToken_IsUrlSafeAndLongEnough()
    {
        var token = TenantBootstrapService.GenerateBearerToken();
        Assert.False(string.IsNullOrEmpty(token));
        Assert.DoesNotContain('+', token);
        Assert.DoesNotContain('/', token);
        Assert.DoesNotContain('=', token);
        Assert.True(token.Length >= 32);
    }

    [Fact]
    public void GenerateBearerToken_ReturnsDistinctValues()
    {
        var a = TenantBootstrapService.GenerateBearerToken();
        var b = TenantBootstrapService.GenerateBearerToken();
        Assert.NotEqual(a, b);
    }
}
