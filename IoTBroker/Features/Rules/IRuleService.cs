using IoTBroker.Domain;
using IoTBroker.Features.Rules.Models;

namespace IoTBroker.Features.Rules;

/// <summary>
/// Service interface for managing and executing sensor rules.
/// </summary>
public interface IRuleService
{
    
    /// <summary>
    /// Adds a new sensor rule to the system.
    /// </summary>
    /// <param name="rule">The sensor rule to add.</param>
    void AddRule(SensorRule rule);
    
    /// <summary>
    /// Retrieves all sensor rules associated with a specific client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>A collection of sensor rules for the specified client.</returns>
    IEnumerable<SensorRule> GetRulesByClient(string clientId);
    
    /// <summary>
    /// Deletes a specific sensor rule for a given client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="ruleId">The identifier of the rule to delete.</param>
    /// <returns>True if the rule was successfully deleted; otherwise, false.</returns>
    bool DeleteRule(string clientId, string ruleId);

    /// <summary>
    /// Evaluates and executes rules based on the incoming sensor payload.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="payload">The sensor payload containing the data to evaluate.</param>
    Task ExecuteRules(string clientId, SensorPayload payload);
}