using System.Net;
using System.Text.Json;
using IoTBroker.Features.ErrorHandling.Models;

namespace IoTBroker.API.Middleware;

/// <summary>
///     Middleware to handle exceptions globally in the application.
///     Catches unhandled exceptions, logs them, and returns a standardized error response to the client.
/// </summary>
public class ExceptionMiddleware
{
    private readonly IHostEnvironment _env;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExceptionMiddleware" /> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger to log exceptions.</param>
    /// <param name="env">The hosting environment to determine if the application is in development mode.</param>
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    /// <summary>
    ///     Invokes the middleware to handle exceptions during the HTTP request processing.
    /// </summary>
    /// <param name="context">The HTTP context of the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    ///     Handles the exception by setting the appropriate HTTP status code and returning a JSON response with error details.
    /// </summary>
    /// <param name="context">The HTTP context of the current request.</param>
    /// <param name="exception">The exception that was caught.</param>
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new ApiErrorResponse
        {
            StatusCode = context.Response.StatusCode,
            Message = _env.IsDevelopment() ? exception.Message : "An internal server error occurred.",
            TraceId = context.TraceIdentifier
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);

        return context.Response.WriteAsync(json);
    }
}