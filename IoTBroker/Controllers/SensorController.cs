using System.Collections.Concurrent;
using IoTBroker.Models;
using Microsoft.AspNetCore.Mvc;

namespace IoTBroker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorController : ControllerBase
{
    // CRUD operations for sensor data can be implemented here

    // In-memory cache for demonstration purposes
    private static readonly ConcurrentDictionary<string, SensorPayload> _store = new();
    private readonly ILogger<SensorController> _logger;

    public SensorController(ILogger<SensorController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Create new sensor data
    /// </summary>
    /// <param name="payload">The sensor data payload</param>
    /// <returns>Action result indicating success or failure</returns>
    [HttpPost]
    public IActionResult CreateSensorData([FromBody] SensorPayload payload)
    {
        if (payload == null) return BadRequest("No data provided.");

        if (payload.Timestamp == default) payload.Timestamp = DateTime.UtcNow;

        switch (payload.Type)
        {
            case SensorType.Numeric:
                if (!double.TryParse(payload.Value, out _))
                    return BadRequest("Value is not a valid number.");
                break;
            case SensorType.Boolean:
                if (!bool.TryParse(payload.Value, out _))
                    return BadRequest("Value is not a valid boolean.");
                break;
            case SensorType.String:
                // No specific validation for string
                break;
            default:
                return BadRequest("Unknown sensor type.");
        }

        // TODO: Consideration of whether a history is needed here or not during refactoring 
        if (!_store.TryAdd(payload.DeviceId, payload))
            return Conflict($"Device {payload.DeviceId} already exists.");

        _logger.LogInformation($"Sensor {payload.DeviceId} created.");
        return CreatedAtAction(nameof(GetSensorData), new { payload.DeviceId }, payload);
    }

    /// <summary>
    ///     Get all sensor data
    /// </summary>
    /// <returns>List of all sensor data</returns>
    [HttpGet]
    public IActionResult GetAllSensorData()
    {
        var list = _store.Values
            .OrderByDescending(x => x.Timestamp)
            .ToList();
        return Ok(list);
    }

    /// <summary>
    ///     Get sensor data by device id
    /// </summary>
    /// <param name="id">The sensor device id</param>
    /// <returns>Sensor data for the specified device id</returns>
    [HttpGet("{id}")]
    public IActionResult GetSensorData(string id)
    {
        // Logic to retrieve sensor data by id

        if (string.IsNullOrWhiteSpace(id)) return BadRequest("Id is required.");

        if (!_store.TryGetValue(id, out var data)) return NotFound($"Device {id} not found.");

        return Ok(data);
    }

    /// <summary>
    ///     Update sensor data by device id
    /// </summary>
    /// <param name="id">The sensor device id</param>
    /// <param name="payload">The updated sensor data payload</param>
    /// <returns>Action result indicating success or failure</returns>
    [HttpPut("{id}")]
    public IActionResult UpdateSensorData(string id, [FromBody] SensorPayload payload)
    {
        // Logic to update sensor data

        if (payload == null) return BadRequest("No data provided.");
        if (string.IsNullOrWhiteSpace(id)) return BadRequest("Id is required.");
        if (string.IsNullOrWhiteSpace(payload.DeviceId)) return BadRequest("DeviceId is required.");

        if (!_store.ContainsKey(id)) return NotFound($"Device {id} not found.");

        // Check if new id already exists if new id is different
        // If payload.DeviceId differs and already exists, conflict
        if (payload.DeviceId != id && _store.ContainsKey(payload.DeviceId))
            return Conflict($"Device {payload.DeviceId} already exists.");

        // Validate type and value
        switch (payload.Type)
        {
            case SensorType.Numeric:
                if (!double.TryParse(payload.Value, out _))
                    return BadRequest("Value is not a valid number.");
                break;
            case SensorType.Boolean:
                if (!bool.TryParse(payload.Value, out _))
                    return BadRequest("Value is not a valid boolean.");
                break;
            case SensorType.String:
                // No specific validation for string
                break;
            default:
                return BadRequest("Unknown sensor type.");
        }

        if (payload.Timestamp == default) payload.Timestamp = DateTime.UtcNow;

        // Remove old key if id changed
        if (payload.DeviceId != id) _store.TryRemove(id, out _);

        _store[payload.DeviceId] = payload;
        _logger.LogInformation("Updated sensor data for {DeviceId}", payload.DeviceId);

        return Ok(payload);
    }

    /// <summary>
    ///     Delete sensor data by device id
    /// </summary>
    /// <param name="id">The sensor device id</param>
    /// <returns>Action result indicating success or failure</returns>
    [HttpDelete("{id}")]
    public IActionResult DeleteSensorData(string id)
    {
        // Logic to delete sensor data
        if (string.IsNullOrWhiteSpace(id)) return BadRequest("Id is required.");

        if (!_store.TryRemove(id, out _)) return NotFound($"Device {id} not found.");

        _logger.LogInformation("Deleted sensor data for {DeviceId}", id);
        return NoContent();
    }
}