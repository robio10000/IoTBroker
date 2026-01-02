using IoTBroker.Rules.Actions;

namespace IoTBroker.Rules.Models;

/// <summary>
/// Represents a complete rule consisting of a trigger condition and associated actions.
/// </summary>
public class SensorRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ClientId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    // Trigger context
    public string TriggerDeviceId { get; set; } = string.Empty;
    public ComparisonOperator Operator { get; set; }
    public string ThresholdValue { get; set; } = string.Empty;
    public bool IgnoreCase { get; set; }

    // What should happen?
    public List<IRuleAction> Actions { get; set; } = new();

    public bool IsActive { get; set; } = true;
}