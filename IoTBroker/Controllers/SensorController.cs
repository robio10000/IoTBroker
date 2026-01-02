using IoTBroker.Models;
using IoTBroker.Services;
using Microsoft.AspNetCore.Mvc;

namespace IoTBroker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorController : ControllerBase
{
    private readonly ILogger<SensorController> _logger;

    private readonly ISensorService _sensorService;

    public SensorController(ILogger<SensorController> logger, ISensorService sensorService)
    {
        _logger = logger;
        _sensorService = sensorService;
    }

    /// <summary>
    ///     Create new sensor data
    /// </summary>
    /// <param name="payload">The sensor data payload</param>
    /// <returns>Action result indicating success or failure</returns>
    [HttpPost]
    public IActionResult CreateSensorData([FromBody] SensorPayload payload)
    {
        // Redundant authentication check
        var client = HttpContext.Items["AuthenticatedClient"] as ApiClient;
        if (client == null)
            return Unauthorized("You must be authenticated to submit sensor data.");

        // Payload check
        if (payload == null) return BadRequest("No data provided.");

        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        // DeviceId check
        if (string.IsNullOrWhiteSpace(payload.DeviceId))
            return BadRequest("DeviceId is required.");

        var result = _sensorService.ProcessPayload(GetClientId(), payload);

        // Service result check
        if (!result.Success)
        {
            if (result.Message.Contains("exists")) return Conflict(result.Message);
            return BadRequest(result.Message);
        }

        _logger.LogInformation("Created new sensor data point for {DeviceId}", payload.DeviceId);
        return CreatedAtAction(nameof(GetSensorData), new { id = payload.DeviceId }, payload);
    }

    /// <summary>
    ///     Get all sensor data
    /// </summary>
    /// <returns>List of all sensor data</returns>
    [HttpGet]
    public IActionResult GetAllSensorData()
    {
        // Admin check
        var client = HttpContext.Items["AuthenticatedClient"] as ApiClient;
        if (client != null && client.Roles.Contains("Admin"))
            return Ok(_sensorService.GetAll(null));

        return Ok(_sensorService.GetAll(GetClientId()));
    }

    /// <summary>
    ///     Get sensor data by device id
    /// </summary>
    /// <param name="id">The sensor device id</param>
    /// <returns>Sensor data for the specified device id</returns>
    [HttpGet("{id}")]
    public IActionResult GetSensorData(string id)
    {
        // User check
        if (!IsAuthorized(id))
            return StatusCode(403, "Forbidden: You do not own this device.");

        var sensor = _sensorService.GetById(GetClientId(), id);
        if (sensor == null) return NotFound($"Device {id} not found.");

        return Ok(sensor);
    }

    /// <summary>
    ///     Get sensor history by device id
    /// </summary>
    /// <param name="id">The sensor device id</param>
    /// <returns>Sensor history for the specified device id</returns>
    [HttpGet("{id}/history")]
    public IActionResult GetSensorHistory(string id)
    {
        // User check
        if (!IsAuthorized(id))
            return StatusCode(403, "Forbidden: You do not own this device.");

        var history = _sensorService.GetHistoryById(GetClientId(), id);
        if (!history.Any()) return NotFound($"No history found for device {id}.");

        return Ok(history);
    }

    /// <summary>
    ///     Delete sensor data by device id
    /// </summary>
    /// <param name="id">The sensor device id</param>
    /// <returns>Action result indicating success or failure</returns>
    [HttpDelete("{id}")]
    public IActionResult DeleteSensorData(string id)
    {
        // User check
        if (!IsAuthorized(id))
            return StatusCode(403, "Forbidden: You do not own this device.");

        if (!_sensorService.Delete(GetClientId(), id)) return NotFound($"Device {id} not found.");

        _logger.LogWarning("Deleted history for device {DeviceId}", id);
        return NoContent();
    }

    private bool IsAuthorized(string deviceId)
    {
        var client = HttpContext.Items["AuthenticatedClient"] as ApiClient;
        if (client == null) return false;

        // Admin can do everything
        if (client.Roles.Contains("Admin")) return true;

        // User can only access their own devices
        return client.OwnedDevices.Contains(deviceId);
    }

    // TODO: *1 Centralize client ID retrieval
    /// <summary>
    ///     Get the authenticated client's ID from the HttpContext
    /// </summary>
    /// <returns>Client ID as string</returns>
    private string GetClientId()
    {
        var client = HttpContext.Items["AuthenticatedClient"] as ApiClient;
        if (client == null) return string.Empty;
        return client.Id;
    }
}