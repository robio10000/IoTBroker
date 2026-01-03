using IoTBroker.Services;
using Microsoft.AspNetCore.Mvc;

namespace IoTBroker.Controllers;

/// <summary>
///     Request model for creating a new API client
/// </summary>
public record CreateClientRequest(string Name, List<string> Roles, HashSet<string>? OwnedDevices);

/// <summary>
///     Controller to manage API clients
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ClientsController : BaseApiController
{
    private readonly IApiKeyService _apiKeyService;

    private readonly ILogger<ClientsController> _logger;

    /// <summary>
    ///     Constructor to initialize the controller with required services
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="apiKeyService">API Key service instance</param>
    public ClientsController(ILogger<ClientsController> logger, IApiKeyService apiKeyService)
    {
        _logger = logger;
        _apiKeyService = apiKeyService;
    }

    /// <summary>
    ///     Check if the authenticated client matches the given client ID
    /// </summary>
    /// <param name="clientId">The client ID to check against</param>
    /// <returns>True if authorized, false otherwise</returns>
    private bool IsAuthorized(string clientId)
    {
        var client = AuthenticatedClient;
        return client != null && client.Id == clientId;
    }

    /// <summary>
    ///     Get all registered API clients
    /// </summary>
    /// <returns>List of ApiClients</returns>
    [HttpGet]
    public IActionResult GetAll()
    {
        if (!IsAuthorized(GetClientId()) && !IsAdmin())
            return StatusCode(403, "Forbidden: You cannot access other client profiles.");

        var clients = _apiKeyService.GetAllClients();
        if (!IsAdmin()) clients = clients.Where(c => c.Id == GetClientId());
        return Ok(clients);
    }

    /// <summary>
    ///     Create a new API client
    /// </summary>
    /// <param name="request">The client creation request payload</param>
    /// <returns>The created ApiClient</returns>
    [HttpPost]
    public IActionResult Create([FromBody] CreateClientRequest request)
    {
        if (!IsAdmin()) return StatusCode(403, "Forbidden: Admin access required.");

        var client = _apiKeyService.CreateClient(request.Name, request.Roles, request.OwnedDevices);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    /// <summary>
    ///     Get an API client by its ID
    /// </summary>
    /// <param name="id">The ID of the client</param>
    /// <returns>The corresponding ApiClient</returns>
    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        // User check (id == client id)
        if (!IsAuthorized(id))
            return StatusCode(403, "Forbidden: You cannot access other client profiles.");

        var client = _apiKeyService.GetClientById(id);
        if (client == null) return NotFound();
        return Ok(client);
    }

    /// <summary>
    ///     Delete (revoke) an API client by its ID
    /// </summary>
    /// <param name="id">The ID of the client to delete</param>
    /// <returns>NoContent if successful, NotFound if client does not exist</returns>
    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        // User check
        if (!IsAdmin()) return StatusCode(403, "Forbidden: Admin access required.");

        var success = _apiKeyService.RevokeClient(id);
        return success ? NoContent() : NotFound();
    }

    [HttpPost("{id}/devices")]
    public IActionResult AddDevice(string id, [FromBody] string deviceId)
    {
        if (!IsAdmin()) return StatusCode(403, "Forbidden: Admin access required.");

        var result = _apiKeyService.AddDeviceToClient(id, deviceId);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
}