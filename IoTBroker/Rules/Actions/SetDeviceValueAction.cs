using IoTBroker.Models;
using IoTBroker.Rules.Models;
using IoTBroker.Services;

namespace IoTBroker.Rules.Actions;

/// <summary>
///     Action to set a device's sensor value.
/// </summary>
public class SetDeviceValueAction : IRuleAction
{
    public string TargetDeviceId { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public SensorType ValueType { get; set; }

    /// <summary>
    ///     Executes the action to set the device's sensor value.
    /// </summary>
    /// <param name="serviceProvider">Used to resolve services like ISensorService.</param>
    /// <param name="clientId">The context of the client who owns the rule.</param>
    /// <param name="triggerPayload">The payload that triggered the rule.</param>
    /// <param name="rule">The rule that triggered this action.</param>
    public void Execute(IServiceProvider serviceProvider, string clientId, SensorPayload triggerPayload, SensorRule rule)
    {
        var sensorService = serviceProvider.GetRequiredService<ISensorService>();

        var payload = new SensorPayload
        {
            DeviceId = TargetDeviceId,
            Type = ValueType,
            Value = NewValue,
            Timestamp = DateTime.UtcNow
        };

        // We re-inject the payload as if it came from a device
        sensorService.ProcessPayload(clientId, payload, true);
    }
}