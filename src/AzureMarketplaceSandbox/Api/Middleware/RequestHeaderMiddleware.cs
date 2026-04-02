namespace AzureMarketplaceSandbox.Api.Middleware;

public class RequestHeaderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (path is not null && path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            if (!context.Request.Headers.TryGetValue("x-ms-requestid", out var requestId) ||
                string.IsNullOrWhiteSpace(requestId))
            {
                requestId = Guid.NewGuid().ToString();
            }

            if (!context.Request.Headers.TryGetValue("x-ms-correlationid", out var correlationId) ||
                string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.Items["x-ms-requestid"] = requestId.ToString();
            context.Items["x-ms-correlationid"] = correlationId.ToString();

            context.Response.OnStarting(() =>
            {
                context.Response.Headers["x-ms-requestid"] = requestId;
                context.Response.Headers["x-ms-correlationid"] = correlationId;
                return Task.CompletedTask;
            });
        }

        await next(context);
    }
}
