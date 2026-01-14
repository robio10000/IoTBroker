using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IoTBroker.Features.Rules.Models;

/// <summary>
///     Represents a condition that must be met for a rule to trigger.
/// </summary>
public class RuleCondition
{
    [Key] [JsonIgnore] public int Id { get; set; }

    public string DeviceId { get; set; } = string.Empty;
    public ComparisonOperator Operator { get; set; }
    public string ThresholdValue { get; set; } = string.Empty;
    public bool IgnoreCase { get; set; }
}