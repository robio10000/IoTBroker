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
    /// Create new sensor data
    /// </summary>
    /// <param name="payload">The sensor data payload</param>
    /// <returns>Action result indicating success or failure</returns>
    [HttpPost]
    public IActionResult CreateSensorData([FromBody] SensorPayload payload)
    {
        // Payload check
        if (payload == null) return BadRequest("No data provided.");
        
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        // DeviceId check
        if (string.IsNullOrWhiteSpace(payload.DeviceId))
            return BadRequest("DeviceId is required.");

        var result = _sensorService.ProcessPayload(payload);

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
    /// Get all sensor data
    /// </summary>
    /// <returns>List of all sensor data</returns>
    [HttpGet]
    public IActionResult GetAllSensorData()
    {
        return Ok(_sensorService.GetAll());
    }

    /// <summary>
    /// Get sensor data by device id
    /// </summary>
    /// <param name="id">The sensor device id</param>
    /// <returns>Sensor data for the specified device id</returns>
    [HttpGet("{id}")]
    public IActionResult GetSensorData(string id)
    {
        var sensor = _sensorService.GetById(id);
        if (sensor == null) return NotFound($"Device {id} not found.");

        return Ok(sensor);
    }

    /// <summary>
    /// Get sensor history by device id
    /// </summary>
    /// <param name="id">The sensor device id</param>
    /// <returns>Sensor history for the specified device id</returns>
    [HttpGet("{id}/history")]
    public IActionResult GetSensorHistory(string id)
    {
        var history = _sensorService.GetHistoryById(id);
        if (!history.Any()) return NotFound($"No history found for device {id}.");

        return Ok(history);
    }

    /// <summary>
    /// Delete sensor data by device id
    /// </summary>
    /// <param name="id">The sensor device id</param>
    /// <returns>Action result indicating success or failure</returns>
    [HttpDelete("{id}")]
    public IActionResult DeleteSensorData(string id)
    {
        if (!_sensorService.Delete(id)) return NotFound($"Device {id} not found.");

        _logger.LogWarning("Deleted history for device {DeviceId}", id);
        return NoContent();
    }
}