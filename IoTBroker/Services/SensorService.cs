using System.Collections.Concurrent;
using IoTBroker.Models;

namespace IoTBroker.Services;

public class SensorService : ISensorService
{
    private readonly ILogger<SensorService> _logger;
    private readonly IApiKeyService _apiKeyService;
    
    private readonly ConcurrentDictionary<string, List<SensorPayload>> _store = new();

    public SensorService(ILogger<SensorService> logger, IApiKeyService apiKeyService)
    {
        _logger = logger;
        _apiKeyService = apiKeyService;
    }

    /// <summary>
    ///     Get all sensor payloads, optionally filtered by client ID
    /// </summary>
    /// <param name="clientId">The client ID to filter by (optional)</param>
    /// <returns>Enumerable of all sensor payloads</returns>
    public IEnumerable<SensorPayload> GetAll(string? clientId)
    {
        // If no clientId is provided, return all payloads
        if (clientId == null)
        {
            var allData = new List<SensorPayload>();

            foreach (var entry in _store)
                lock (entry.Value)
                {
                    allData.AddRange(entry.Value);
                }

            return allData.OrderByDescending(x => x.Timestamp);
        }

        // Filter payloads by clientId
        var filteredData = new List<SensorPayload>();
        foreach (var entry in _store)
            if (entry.Key.StartsWith(clientId + "_"))
                lock (entry.Value)
                {
                    filteredData.AddRange(entry.Value);
                }

        return filteredData.OrderByDescending(x => x.Timestamp);
    }

    /// <summary>
    ///     Get the latest sensor payload by device ID
    /// </summary>
    /// <param name="id">The sensor device id</param>
    /// <returns>The latest sensor payload or null if not found</returns>
    public SensorPayload? GetById(string clientId, string id)
    {
        var internalKey = GetInternalKey(clientId, id);

        if (_store.TryGetValue(internalKey, out var history))
            lock (history)
            {
                return history.OrderByDescending(x => x.Timestamp).FirstOrDefault();
            }

        return null;
    }

    /// <summary>
    ///     Get the full history of sensor payloads by device ID
    /// </summary>
    /// <param name="id">The sensor device id</param>
    /// <returns>Enumerable of sensor payloads</returns>
    public IEnumerable<SensorPayload> GetHistoryById(string clientId, string id)
    {
        var internalKey = GetInternalKey(clientId, id);

        if (_store.TryGetValue(internalKey, out var history))
            lock (history)
            {
                return history.OrderByDescending(x => x.Timestamp).ToList();
            }

        return Enumerable.Empty<SensorPayload>();
    }

    /// <summary>
    ///     Check if a device ID exists in the store
    /// </summary>
    /// <param name="id">The sensor id</param>
    /// <returns>True if exists, false otherwise</returns>
    public bool Exists(string clientId, string id)
    {
        var internalKey = GetInternalKey(clientId, id);
        return _store.ContainsKey(internalKey);
    }

    /// <summary>
    ///     Delete a device history by ID
    /// </summary>
    /// <param name="id">The sensor id</param>
    /// <returns>True if deleted, false otherwise</returns>
    public bool Delete(string clientId, string id)
    {
        var internalKey = GetInternalKey(clientId, id);
        return _store.TryRemove(internalKey, out _);
    }

    /// <summary>
    ///     Process and store the sensor payload
    /// </summary>
    /// <param name="payload">The sensor payload</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    public ServiceResult ProcessPayload(string clientId, SensorPayload payload)
    {
        // 1. Validation of the payload
        var validation = ValidateValue(payload);
        if (!validation.Success) return validation;
        
        // Ensure the device is registered to the client
        _apiKeyService.AddDeviceToClient(clientId, payload.DeviceId);
        
        // 2. Get or create the device history list
        var internalKey = GetInternalKey(clientId, payload.DeviceId);
        var deviceHistory = _store.GetOrAdd(internalKey, _ => new List<SensorPayload>());

        // 3. Thread-safe addition to the device history
        lock (deviceHistory)
        {
            // Check for duplicate timestamps
            if (deviceHistory.Any(x => x.Timestamp == payload.Timestamp))
                return new ServiceResult(false, "Data for this timestamp already exists.");

            deviceHistory.Add(payload);
        }

        _logger.LogInformation($"Data processed for device {payload.DeviceId} by client {clientId}.");
        return new ServiceResult(true);
    }

    /// <summary>
    ///     Validate the sensor payload value based on its type
    /// </summary>
    /// <param name="payload">The sensor payload</param>
    /// <returns>ServiceResult indicating validation success or failure</returns>
    private ServiceResult ValidateValue(SensorPayload payload)
    {
        switch (payload.Type)
        {
            case SensorType.Numeric:
                return double.TryParse(payload.Value, out _)
                    ? new ServiceResult(true)
                    : new ServiceResult(false, "Value is not a valid number.");
            case SensorType.Boolean:
                return bool.TryParse(payload.Value, out _)
                    ? new ServiceResult(true)
                    : new ServiceResult(false, "Value is not a valid boolean.");
            case SensorType.String:
                return new ServiceResult(true);
            default:
                return new ServiceResult(false, "Unknown sensor type.");
        }
    }

    /// <summary>
    ///     Generate an internal key for storage based on client and device IDs
    /// </summary>
    /// <param name="clientId">The client ID</param>
    /// <param name="deviceId">The device ID</param>
    /// <returns>Internal storage key</returns>
    private string GetInternalKey(string clientId, string deviceId)
    {
        return $"{clientId}_{deviceId}";
    }
}