using System.Collections.Concurrent;
using IoTBroker.Models;
using IoTBroker.Rules.Models;
using IoTBroker.Rules.Strategies;

namespace IoTBroker.Rules;

/// <summary>
///     Service for managing and executing sensor rules.
/// </summary>
public class RuleService : IRuleService
{
    private readonly ConcurrentDictionary<string, SensorRule> _rules = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<ITriggerStrategy> _strategies;

    public RuleService(IEnumerable<ITriggerStrategy> strategies, IServiceProvider serviceProvider)
    {
        _strategies = strategies;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     Adds a new sensor rule.
    /// </summary>
    /// <param name="rule">The sensor rule to add.</param>
    public void AddRule(SensorRule rule)
    {
        _rules.TryAdd(rule.Id, rule);
    }

    /// <summary>
    ///     Retrieves all sensor rules for a specific client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>A collection of sensor rules for the specified client.</returns>
    public IEnumerable<SensorRule> GetRulesByClient(string clientId)
    {
        return _rules.Values.Where(r => r.ClientId == clientId);
    }

    /// <summary>
    ///     Deletes a sensor rule for a specific client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="ruleId">The identifier of the rule to delete.</param>
    /// <returns>True if the rule was successfully deleted; otherwise, false.</returns>
    public bool DeleteRule(string clientId, string ruleId)
    {
        if (_rules.TryGetValue(ruleId, out var rule) && rule.ClientId == clientId)
            return _rules.TryRemove(ruleId, out _);
        return false;
    }

    /// <summary>
    ///     Evaluates and executes rules based on the incoming sensor payload.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="payload">The sensor payload containing the data to evaluate.</param>
    public void ExecuteRules(string clientId, SensorPayload payload)
    {
        // 1. Search for all active rules for this client and device
        var matchingRules = _rules.Values
            .Where(r => r.ClientId == clientId &&
                        r.TriggerDeviceId == payload.DeviceId &&
                        r.IsActive);

        foreach (var rule in matchingRules)
        {
            // 2. Find the appropriate strategy for the sensor type (Numeric, String, etc.)
            var strategy = _strategies.FirstOrDefault(s => s.SupportedType == payload.Type);

            if (strategy == null) continue;

            // 3. Evaluate the condition
            if (strategy.Evaluate(payload.Value, rule.Operator, rule.ThresholdValue, rule.IgnoreCase))
                // 4. Execute all linked actions
                foreach (var action in rule.Actions)
                    action.Execute(_serviceProvider, clientId);
        }
    }
}