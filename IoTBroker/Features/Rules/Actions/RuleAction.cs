using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IoTBroker.Domain;
using IoTBroker.Features.Rules.Models;

namespace IoTBroker.Features.Rules.Actions;

/// <summary>
///     Base class for all rule actions.
/// </summary>

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(SetDeviceValueAction), "set_value")]
[JsonDerivedType(typeof(WebHookAction), "webhook")]
public abstract class RuleAction
{
    [Key] [JsonIgnore] public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Executes the action.
    /// </summary>
    /// <param name="serviceProvider">Used to resolve services like ISensorService.</param>
    /// <param name="clientId">The context of the client who owns the rule.</param>
    /// <param name="triggerPayload">The payload that triggered the rule.</param>
    /// <param name="rule">The rule that triggered this action.</param>
    public abstract Task ExecuteAsync(IServiceProvider serviceProvider, string clientId, SensorPayload triggerPayload,
        SensorRule rule);
}