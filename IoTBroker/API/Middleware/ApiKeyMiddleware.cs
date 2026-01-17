using IoTBroker.Features.Clients;

namespace IoTBroker.API.Middleware;

/// <summary>
///     Middleware to handle API Key authentication
/// </summary>
public class ApiKeyMiddleware
{
    private const string ApiKeyHeaderName = "X-API-KEY";
    private readonly RequestDelegate _next;

    // TODO: Caching of valid API keys to reduce database lookups
    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    ///     Invoke the middleware to check for API Key in the request headers
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="apiKeyService">The API Key service to validate keys</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        // Ignore api key check for documentation and health endpoints to allow public access
        if (context.Request.Path.StartsWithSegments("/doc") || context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("API Key is missing.");
            return;
        }

        var client = await apiKeyService.GetClientByKey(extractedApiKey);
        if (client == null)
        {
            context.Response.StatusCode = 401;
            return;
        }

        context.Items["AuthenticatedClient"] = client;

        await _next(context);
    }
}