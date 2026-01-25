using System.ComponentModel.DataAnnotations;

namespace IoTBroker.Domain;

/// <summary>
///     Represents the state of a device in the IoT system.
/// </summary>
public class DeviceState
{
    public string ClientId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public SensorType Type { get; set; }
    public DateTime LastUpdate { get; set; }
}