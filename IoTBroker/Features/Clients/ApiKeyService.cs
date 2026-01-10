using System.Collections.Concurrent;
using IoTBroker.Domain;
using IoTBroker.Features.Sensors;

namespace IoTBroker.Features.Clients;

/// <summary>
///     Service to manage API clients and their API keys
/// </summary>
public class ApiKeyService : IApiKeyService
{
    private readonly ConcurrentDictionary<string, ApiClient> _clients = new();

    private readonly ILogger<ApiKeyService> _logger;

    /// <summary>
    ///     Constructor initializes the service with test clients
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public ApiKeyService(ILogger<ApiKeyService> logger)
    {
        _logger = logger;

        var testClient = new ApiClient
        {
            Id = "test-id",
            Name = "Dev-Simulator",
            ApiKey = "client-key-123",
            Roles = new List<string> { "SensorNode" }
        };

        var testAdminClient = new ApiClient
        {
            Id = "admin-id",
            Name = "Dev-Admin",
            ApiKey = "admin-key-123",
            Roles = new List<string> { "Admin" }
        };

        _clients[testClient.Id] = testClient;
        _clients[testAdminClient.Id] = testAdminClient;
    }

    /// <summary>
    ///     Retrieve an API client by its API key
    /// </summary>
    /// <param name="apiKey">The API key to look up</param>
    /// <returns>The corresponding ApiClient or null if not found</returns>
    public ApiClient? GetClientByKey(string apiKey)
    {
        return _clients.FirstOrDefault(c => c.Value.ApiKey == apiKey).Value;
    }

    /// <summary>
    ///     Create a new API client with specified name, roles, and owned devices
    /// </summary>
    /// <param name="name">The name of the client</param>
    /// <param name="roles">The roles assigned to the client</param>
    /// <param name="ownedDevices">Optional set of device IDs owned by the client</param>
    /// <returns>The newly created ApiClient</returns>
    public ApiClient CreateClient(string name, List<string> roles, HashSet<string>? ownedDevices = null)
    {
        var client = new ApiClient
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            ApiKey = Guid.NewGuid().ToString(),
            Roles = roles,
            OwnedDevices = ownedDevices ?? new HashSet<string>()
        };
        _clients[client.Id] = client;

        _logger.LogInformation($"Client {client.Id} created");
        return client;
    }

    /// <summary>
    ///     Revoke an existing API client by its ID
    /// </summary>
    /// <param name="clientId">The ID of the client to revoke</param>
    /// <returns>True if the client was successfully revoked, false otherwise</returns>
    public bool RevokeClient(string clientId)
    {
        _logger.LogInformation($"Revoking client {clientId}");
        // TODO: Remove cascaded data (e.g. sensor payloads)
        return _clients.TryRemove(clientId, out _);
    }

    /// <summary>
    ///     Retrieve all registered API clients
    /// </summary>
    /// <returns>Enumerable of all ApiClients</returns>
    public IEnumerable<ApiClient> GetAllClients()
    {
        return _clients.Values;
    }

    /// <summary>
    ///     Retrieve an API client by its ID
    /// </summary>
    /// <param name="clientId">The ID of the client to look up</param>
    /// <returns>The corresponding ApiClient or null if not found</returns>
    public ApiClient? GetClientById(string clientId)
    {
        _clients.TryGetValue(clientId, out var client);
        return client;
    }

    /// <summary>
    ///     Update the roles assigned to a specific client
    /// </summary>
    /// <param name="clientId">The ID of the client to update</param>
    /// <param name="roles">The new list of roles to assign</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    public ServiceResult UpdateClientRoles(string clientId, List<string> roles)
    {
        _clients.TryGetValue(clientId, out var client);
        if (client == null) return new ServiceResult(false, "Client not found");

        client.Roles = roles;
        _clients[clientId] = client;
        return new ServiceResult(true, "Client roles updated");
    }

    /// <summary>
    ///     Update the set of owned devices for a specific client
    /// </summary>
    /// <param name="clientId">The ID of the client to update</param>
    /// <param name="ownedDevices">The new set of owned device IDs</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    public ServiceResult UpdateClientOwnedDevices(string clientId, HashSet<string> ownedDevices)
    {
        _clients.TryGetValue(clientId, out var client);
        if (client == null) return new ServiceResult(false, "Client not found");

        client.OwnedDevices = ownedDevices;
        _clients[clientId] = client;
        return new ServiceResult(true, "Client owned devices updated");
    }

    /// <summary>
    ///     Add a device to a client's owned devices
    /// </summary>
    /// <param name="clientId">The ID of the client</param>
    /// <param name="deviceId">The ID of the device to add</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    public ServiceResult AddDeviceToClient(string clientId, string deviceId)
    {
        _clients.TryGetValue(clientId, out var client);
        if (client == null) return new ServiceResult(false, "Client not found");

        lock (client)
        {
            if (client.OwnedDevices.Add(deviceId)) return new ServiceResult(true, "Device added to client");

            return new ServiceResult(false, "Device already assigned to client");
        }
    }

    /// <summary>
    ///     Remove a device from a client's owned devices
    /// </summary>
    /// <param name="clientId">The ID of the client</param>
    /// <param name="deviceId">The ID of the device to remove</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    public ServiceResult RemoveDeviceFromClient(string clientId, string deviceId)
    {
        if (!IsClientAuthorizedForDevice(clientId, deviceId))
            return new ServiceResult(false, "Client not authorized for this device");
        _clients.TryGetValue(clientId, out var client);
        if (client == null) return new ServiceResult(false, "Client not found");

        if (!client.OwnedDevices.Contains(deviceId)) return new ServiceResult(false, "Device not assigned to client");

        lock (client)
        {
            if (client.OwnedDevices.Remove(deviceId)) return new ServiceResult(true, "Device removed from client");
            return new ServiceResult(false, "Device not assigned to client");
        }
    }

    /// <summary>
    ///     Check if a client is authorized for a specific device
    /// </summary>
    /// <param name="clientId">The ID of the client</param>
    /// <param name="deviceId">The ID of the device</param>
    /// <returns>True if authorized, false otherwise</returns>
    public bool IsClientAuthorizedForDevice(string clientId, string deviceId)
    {
        _clients.TryGetValue(clientId, out var client);
        if (client == null) return false;

        // Admin do everything
        if (client.Roles.Contains("Admin")) return true;

        // User can only access their own devices
        return client.OwnedDevices.Contains(deviceId);
    }
}