using IoTBroker.Models;

namespace IoTBroker.Services;

/// <summary>
///     Result of a service operation
/// </summary>
public record ServiceResult(bool Success, string Message = "");

/// <summary>
///     Service interface for managing sensor data
/// </summary>
public interface ISensorService
{
    /// <summary>
    ///     Get all sensor payloads, optionally filtered by client ID
    /// </summary>
    /// <param name="clientId">The client ID to filter by (optional)</param>
    /// <returns>Enumerable of all sensor payloads</returns>
    IEnumerable<SensorPayload> GetAll(string? clientId);

    /// <summary>
    ///     Get the latest sensor payload by device ID
    /// </summary>
    /// <param name="id">The sensor device id</param>
    /// <returns>The latest sensor payload or null if not found</returns>
    SensorPayload? GetById(string clientId, string id);

    /// <summary>
    ///     Get the history of sensor payloads by device ID
    /// </summary>
    /// <param name="id">The sensor device id</param>
    /// <returns>Enumerable of sensor payloads for the specified device id</returns>
    public IEnumerable<SensorPayload> GetHistoryById(string clientId, string id);

    /// <summary>
    ///     Check if a sensor payload exists for the given client and device ID
    /// </summary>
    /// <param name="clientId">The client ID</param>
    /// <param name="id">The sensor device id</param>
    /// <returns>True if the sensor payload exists, false otherwise</returns>
    bool Exists(string clientId, string id);

    /// <summary>
    ///     Delete a sensor payload for the given client and device ID
    /// </summary>
    /// <param name="clientId">The client ID</param>
    /// <param name="id">The sensor device id</param>
    /// <returns>True if the sensor payload was deleted, false otherwise</returns>
    bool Delete(string clientId, string id);

    /// <summary>
    ///     Process and store a new sensor payload
    /// </summary>
    /// <param name="clientId">The client ID submitting the payload</param>
    /// <param name="payload">The sensor payload to process</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    ServiceResult ProcessPayload(string clientId, SensorPayload payload);
}