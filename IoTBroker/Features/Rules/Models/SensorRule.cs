using System.ComponentModel;
using System.Text.Json.Serialization;
using IoTBroker.Features.Rules.Actions;

namespace IoTBroker.Features.Rules.Models;

/// <summary>
///     Represents a complete rule consisting of a trigger condition and associated actions.
/// </summary>
public class SensorRule
{
    [ReadOnly(true)] public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonIgnore] public string ClientId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<RuleCondition> Conditions { get; set; } = new();

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.All;

    // What should happen?
    public List<IRuleAction> Actions { get; set; } = new();

    public bool IsActive { get; set; } = true;

    [ReadOnly(true)] public DateTime? LastTriggered { get; set; }
}