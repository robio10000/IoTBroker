using System.Text.Json.Serialization;
using IoTBroker.Domain;
using IoTBroker.Features.Rules.Models;

namespace IoTBroker.Features.Rules.Actions;

/// <summary>
///     Defines an action that can be executed when a rule trigger is met.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(SetDeviceValueAction), "set_value")]
[JsonDerivedType(typeof(WebHookAction), "webhook")]
public interface IRuleAction
{
    /// <summary>
    ///     Executes the action.
    /// </summary>
    /// <param name="serviceProvider">Used to resolve services like ISensorService.</param>
    /// <param name="clientId">The context of the client who owns the rule.</param>
    /// <param name="triggerPayload">The payload that triggered the rule.</param>
    /// <param name="rule">The rule that triggered this action.</param>
    void Execute(IServiceProvider serviceProvider, string clientId, SensorPayload triggerPayload, SensorRule rule);
}