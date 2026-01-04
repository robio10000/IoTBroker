namespace IoTBroker.Rules.Models;

/// <summary>
///     Represents a condition that must be met for a rule to trigger.
/// </summary>
public class RuleCondition
{
    public string DeviceId { get; set; } = string.Empty;
    public ComparisonOperator Operator { get; set; }
    public string ThresholdValue { get; set; } = string.Empty;
    public bool IgnoreCase { get; set; }
}