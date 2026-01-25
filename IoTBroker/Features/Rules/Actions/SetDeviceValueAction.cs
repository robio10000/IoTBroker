using IoTBroker.Domain;
using IoTBroker.Features.Rules.Helper;
using IoTBroker.Features.Rules.Models;
using IoTBroker.Features.Sensors;

namespace IoTBroker.Features.Rules.Actions;

/// <summary>
///     Action to set a device's sensor value.
/// </summary>
public class SetDeviceValueAction : RuleAction //IRuleAction
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
    public override Task ExecuteAsync(IServiceProvider serviceProvider, string clientId, SensorPayload triggerPayload,
        SensorRule rule)
    {
        var sensorService = serviceProvider.GetRequiredService<ISensorService>();

        var finalValue = TokenReplacer.Replace(NewValue, triggerPayload, rule);

        var payload = new SensorPayload
        {
            DeviceId = TargetDeviceId,
            Type = ValueType,
            Value = finalValue ?? NewValue,
            Timestamp = DateTime.UtcNow
        };

        // We re-inject the payload as if it came from a device
        sensorService.ProcessPayload(clientId, payload, true);
        return Task.CompletedTask;
    }
}