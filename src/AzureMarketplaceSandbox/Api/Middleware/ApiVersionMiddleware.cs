namespace AzureMarketplaceSandbox.Api.Middleware;

public class ApiVersionMiddleware(RequestDelegate next)
{
    private const string RequiredVersion = "2018-08-31";

    private static readonly string[] ApiPrefixes =
    [
        "/api/saas/",
        "/api/usageEvent",
        "/api/batchUsageEvent",
        "/api/usageEvents"
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (path is not null && ApiPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            if (!context.Request.Query.TryGetValue("api-version", out var version) ||
                version.ToString() != RequiredVersion)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = $"api-version query parameter is required and must be '{RequiredVersion}'.",
                    code = "BadArgument"
                });
                return;
            }
        }

        await next(context);
    }
}
