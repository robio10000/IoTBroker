using IoTBroker.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IoTBroker.API.Controllers;

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
        return AuthenticatedClient?.Id ?? string.Empty;
    }

    /// <summary>
    ///     Helper to check if the current client is an Admin.
    /// </summary>
    protected bool IsAdmin()
    {
        return AuthenticatedClient?.Roles.Contains("Admin") ?? false;
    }
}