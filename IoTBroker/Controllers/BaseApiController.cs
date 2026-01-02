using IoTBroker.Models;
using Microsoft.AspNetCore.Mvc;

namespace IoTBroker.Controllers;

/// <summary>
///     Base API controller providing common functionality for derived controllers.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    
    /// <summary>
    ///     Gets the authenticated ApiClient from the HttpContext items.
    /// </summary>
    protected ApiClient? AuthenticatedClient => HttpContext.Items["AuthenticatedClient"] as ApiClient;
    
    /// <summary>
    ///     Gets the authenticated client's ID from the HttpContext items.
    /// </summary>
    /// <returns>The Client ID or string.Empty if not authenticated.</returns>
    protected string GetClientId()
    {
        var client = HttpContext.Items["AuthenticatedClient"] as ApiClient;
        return client?.Id ?? string.Empty;
    }

    /// <summary>
    ///     Helper to check if the current client is an Admin.
    /// </summary>
    protected bool IsAdmin()
    {
        var client = HttpContext.Items["AuthenticatedClient"] as ApiClient;
        return client?.Roles.Contains("Admin") ?? false;
    }
}