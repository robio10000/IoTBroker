using System.ComponentModel.DataAnnotations;

namespace IoTBroker.Domain;

/// <summary>
///     Represents the payload sent by a sensor device.
/// </summary>
public class SensorPayload
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string DeviceId { get; set; } = string.Empty;
    
    [Required]
    public SensorType Type { get; set; }
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}