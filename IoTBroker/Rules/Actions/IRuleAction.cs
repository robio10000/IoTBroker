using System.Text.Json.Serialization;

namespace IoTBroker.Rules.Actions;

/// <summary>
///     Defines an action that can be executed when a rule trigger is met.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(SetDeviceValueAction), "set_value")]
public interface IRuleAction
{
    /// <summary>
    ///     Executes the action.
    /// </summary>
    /// <param name="serviceProvider">Used to resolve services like ISensorService.</param>
    /// <param name="clientId">The context of the client who owns the rule.</param>
    void Execute(IServiceProvider serviceProvider, string clientId);
}