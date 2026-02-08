namespace IoTBroker.Features.ErrorHandling.Models;

/// <summary>
///     Represents the structure of an API error response.
///     Contains the status code, error message, and an optional trace identifier for debugging purposes.
/// </summary>
public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TraceId { get; set; }
}