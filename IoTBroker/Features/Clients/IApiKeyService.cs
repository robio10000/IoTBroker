using IoTBroker.Models;

namespace IoTBroker.Services;

/// <summary>
///     Service interface for managing API keys and clients
/// </summary>
public interface IApiKeyService
{
    /// <summary>
    ///     Retrieve an API client by its API key
    /// </summary>
    /// <param name="apiKey">The API key to look up</param>
    /// <returns>The corresponding ApiClient or null if not found</returns>
    ApiClient? GetClientByKey(string apiKey);

    /// <summary>
    ///     Create a new API client with specified name, roles, and owned devices
    /// </summary>
    /// <param name="name">The name of the client</param>
    /// <param name="roles">The roles assigned to the client</param>
    /// <param name="ownedDevices">Optional set of device IDs owned by the client</param>
    /// <returns>The newly created ApiClient</returns>
    ApiClient CreateClient(string name, List<string> roles, HashSet<string>? ownedDevices = null);

    /// <summary>
    ///     Revoke an existing API client by its ID
    /// </summary>
    /// <param name="clientId">The ID of the client to revoke</param>
    /// <returns>True if the client was successfully revoked, false otherwise</returns>
    bool RevokeClient(string clientId);

    /// <summary>
    ///     Retrieve all registered API clients
    /// </summary>
    /// <returns>Enumerable of all ApiClients</returns>
    IEnumerable<ApiClient> GetAllClients();

    /// <summary>
    ///     Retrieve an API client by its ID
    /// </summary>
    /// <param name="clientId">The ID of the client to look up</param>
    /// <returns>The corresponding ApiClient or null if not found</returns>
    ApiClient? GetClientById(string clientId);

    /// <summary>
    ///     Update the roles assigned to a specific client
    /// </summary>
    /// <param name="clientId">The ID of the client to update</param>
    /// <param name="roles">The new list of roles to assign</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    ServiceResult UpdateClientRoles(string clientId, List<string> roles);

    /// <summary>
    ///     Update the set of owned devices for a specific client
    /// </summary>
    /// <param name="clientId">The ID of the client to update</param>
    /// <param name="ownedDevices">The new set of owned device IDs</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    ServiceResult UpdateClientOwnedDevices(string clientId, HashSet<string> ownedDevices);

    /// <summary>
    ///     Add a device to a client's owned devices
    /// </summary>
    /// <param name="clientId">The ID of the client</param>
    /// <param name="deviceId">The ID of the device to add</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    ServiceResult AddDeviceToClient(string clientId, string deviceId);

    /// <summary>
    ///     Remove a device from a client's owned devices
    /// </summary>
    /// <param name="clientId">The ID of the client</param>
    /// <param name="deviceId">The ID of the device to remove</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    ServiceResult RemoveDeviceFromClient(string clientId, string deviceId);

    /// <summary>
    ///     Check if a client is authorized for a specific device
    /// </summary>
    /// <param name="clientId">The ID of the client</param>
    /// <param name="deviceId">The ID of the device</param>
    /// <returns>True if authorized, false otherwise</returns>
    bool IsClientAuthorizedForDevice(string clientId, string deviceId);
}