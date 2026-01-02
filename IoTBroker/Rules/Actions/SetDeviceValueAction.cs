using IoTBroker.Models;
using IoTBroker.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IoTBroker.Rules.Actions;

public class SetDeviceValueAction : IRuleAction
{
    public string TargetDeviceId { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public SensorType ValueType { get; set; }

    public void Execute(IServiceProvider serviceProvider, string clientId)
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
        sensorService.ProcessPayload(clientId, payload);
    }
}