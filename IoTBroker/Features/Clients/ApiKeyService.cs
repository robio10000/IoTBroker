using IoTBroker.Domain;
using IoTBroker.Features.Sensors;
using IoTBroker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IoTBroker.Features.Clients;

/// <summary>
///     Service to manage API clients and their API keys
/// </summary>
public class ApiKeyService : IApiKeyService
{
    private readonly IoTContext _context;

    private readonly ILogger<ApiKeyService> _logger;

    /// <summary>
    ///     Constructor initializes the service with test clients
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="context">Database context</param>
    public ApiKeyService(ILogger<ApiKeyService> logger, IoTContext context)
    {
        _logger = logger;
        _context = context;

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


        if (!_context.Clients.Any(c => c.Id == testClient.Id))
        {
            _context.Clients.Add(testClient);
            _logger.LogInformation("Added test API client with ID 'test-id' and API key 'client-key-123'");
        }

        if (!_context.Clients.Any(c => c.Id == testAdminClient.Id))
        {
            _context.Clients.Add(testAdminClient);
            _logger.LogInformation("Added test Admin API client with ID 'admin-id' and API key 'admin-key-123'");
        }

        _context.SaveChanges();
    }

    /// <summary>
    ///     Retrieve an API client by its API key
    /// </summary>
    /// <param name="apiKey">The API key to look up</param>
    /// <returns>The corresponding ApiClient or null if not found</returns>
    public async Task<ApiClient?> GetClientByKey(string apiKey)
    {
        return await _context.Clients.FirstOrDefaultAsync(c => c.ApiKey == apiKey);
    }

    /// <summary>
    ///     Create a new API client with specified name, roles, and owned devices
    /// </summary>
    /// <param name="name">The name of the client</param>
    /// <param name="roles">The roles assigned to the client</param>
    /// <param name="ownedDevices">Optional set of device IDs owned by the client</param>
    /// <returns>The newly created ApiClient</returns>
    public async Task<ApiClient> CreateClient(string name, List<string> roles, HashSet<string>? ownedDevices = null)
    {
        var client = new ApiClient
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            ApiKey = Guid.NewGuid().ToString(),
            Roles = roles,
            OwnedDevices = ownedDevices ?? new HashSet<string>()
        };
        
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Client {client.Id} created");
        return client;
    }

    /// <summary>
    ///     Revoke an existing API client by its ID
    /// </summary>
    /// <param name="clientId">The ID of the client to revoke</param>
    /// <returns>True if the client was successfully revoked, false otherwise</returns>
    public async Task<bool> RevokeClient(string clientId)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null) return false;
        _context.Clients.Attach(client);
        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Revoked client {clientId}");
        return true;
    }

    /// <summary>
    ///     Retrieve all registered API clients
    /// </summary>
    /// <returns>Enumerable of all ApiClients</returns>
    public async Task<IEnumerable<ApiClient>> GetAllClients()
    {
        return await _context.Clients.ToListAsync();
    }

    /// <summary>
    ///     Retrieve an API client by its ID
    /// </summary>
    /// <param name="clientId">The ID of the client to look up</param>
    /// <returns>The corresponding ApiClient or null if not found</returns>
    public async Task<ApiClient?> GetClientById(string clientId)
    {
        return await _context.Clients.FindAsync(clientId);
    }

    /// <summary>
    ///     Update the roles assigned to a specific client
    /// </summary>
    /// <param name="clientId">The ID of the client to update</param>
    /// <param name="roles">The new list of roles to assign</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    public async Task<ServiceResult> UpdateClientRoles(string clientId, List<string> roles)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null) return new ServiceResult(false, "Client not found");

        client.Roles = roles;
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
        return new ServiceResult(true, "Client roles updated");
    }

    /// <summary>
    ///     Update the set of owned devices for a specific client
    /// </summary>
    /// <param name="clientId">The ID of the client to update</param>
    /// <param name="ownedDevices">The new set of owned device IDs</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    public async Task<ServiceResult> UpdateClientOwnedDevices(string clientId, HashSet<string> ownedDevices)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null) return new ServiceResult(false, "Client not found");
        client.OwnedDevices = ownedDevices;
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
        return new ServiceResult(true, "Client owned devices updated");
    }

    /// <summary>
    ///     Add a device to a client's owned devices
    /// </summary>
    /// <param name="clientId">The ID of the client</param>
    /// <param name="deviceId">The ID of the device to add</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    public async Task<ServiceResult> AddDeviceToClient(string clientId, string deviceId)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null) return new ServiceResult(false, "Client not found");
        if (client.OwnedDevices.Add(deviceId))
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
            return new ServiceResult(true, "Device added to client");
        }

        return new ServiceResult(false, "Device already assigned to client");
    }

    /// <summary>
    ///     Remove a device from a client's owned devices
    /// </summary>
    /// <param name="clientId">The ID of the client</param>
    /// <param name="deviceId">The ID of the device to remove</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    public async Task<ServiceResult> RemoveDeviceFromClient(string clientId, string deviceId)
    {
        if (!await IsClientAuthorizedForDevice(clientId, deviceId))
            return new ServiceResult(false, "Client not authorized for this device");
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null) return new ServiceResult(false, "Client not found");
        if (!client.OwnedDevices.Contains(deviceId)) return new ServiceResult(false, "Device not assigned to client");
        if (client.OwnedDevices.Remove(deviceId))
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
            return new ServiceResult(true, "Device removed from client");
        }

        return new ServiceResult(false, "Device not assigned to client");
    }

    /// <summary>
    ///     Check if a client is authorized for a specific device
    /// </summary>
    /// <param name="clientId">The ID of the client</param>
    /// <param name="deviceId">The ID of the device</param>
    /// <returns>True if authorized, false otherwise</returns>
    public async Task<bool> IsClientAuthorizedForDevice(string clientId, string deviceId)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null) return false;

        // Admin do everything
        if (client.Roles.Contains("Admin")) return true;

        // User can only access their own devices
        return client.OwnedDevices.Contains(deviceId);
    }
}