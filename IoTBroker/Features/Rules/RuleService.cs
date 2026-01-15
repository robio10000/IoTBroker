using IoTBroker.Domain;
using IoTBroker.Features.Rules.Models;
using IoTBroker.Features.Rules.Strategies;
using IoTBroker.Features.Sensors;
using IoTBroker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IoTBroker.Features.Rules;

/// <summary>
///     Service for managing and executing sensor rules.
/// </summary>
public class RuleService : IRuleService
{
    private readonly IoTContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<ITriggerStrategy> _strategies;

    public RuleService(IEnumerable<ITriggerStrategy> strategies, IServiceProvider serviceProvider, IoTContext context)
    {
        _strategies = strategies;
        _serviceProvider = serviceProvider;
        _context = context;
    }

    /// <summary>
    ///     Adds a new sensor rule.
    /// </summary>
    /// <param name="rule">The sensor rule to add.</param>
    public async Task AddRule(SensorRule rule)
    {
        _context.Rules.Add(rule);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Retrieves all sensor rules for a specific client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>A collection of sensor rules for the specified client.</returns>
    public async Task<IEnumerable<SensorRule>> GetRulesByClient(string clientId)
    {
        return await _context.Rules.Where(r => r.ClientId == clientId).ToListAsync();
    }

    /// <summary>
    ///     Deletes a sensor rule for a specific client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="ruleId">The identifier of the rule to delete.</param>
    /// <returns>True if the rule was successfully deleted; otherwise, false.</returns>
    public async Task<bool> DeleteRule(string clientId, string ruleId)
    {
        if (await _context.Rules.AnyAsync(r => r.Id == ruleId && r.ClientId == clientId))
        {
            var rule = new SensorRule { Id = ruleId, ClientId = clientId };
            _context.Rules.Attach(rule);
            _context.Rules.Remove(rule);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    /// <summary>
    ///     Evaluates and executes rules based on the incoming sensor payload.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="payload">The sensor payload containing the data to evaluate.</param>
    public async Task ExecuteRules(string clientId, SensorPayload payload)
    {
        // 1. Search for relevant rules for the client and device in any condition
        var relevantRules = await _context.Rules
            .Include(r => r.Conditions)
            .Include(r => r.Actions)
            .Where(r => r.ClientId == clientId &&
                        r.IsActive &&
                        r.Conditions.Any(c => c.DeviceId == payload.DeviceId))
            .ToListAsync();
        
        var sensorService = _serviceProvider.GetRequiredService<ISensorService>();
        
        foreach (var rule in relevantRules)
        {
            // Initial status depending on logical operator
            // All -> true, Any -> false
            var isTriggered = rule.LogicalOperator == LogicalOperator.All;

            foreach (var condition in rule.Conditions)
            {
                var conditionMet = false;

                // If the device in the condition is the currently sending device -> use payload
                if (condition.DeviceId == payload.DeviceId)
                {
                    conditionMet = EvaluateCondition(condition, payload.Value, payload.Type);
                }
                else
                {
                    // If the device in the condition is different -> fetch last state from SensorService
                    // Use the ServiceProvider to avoid circular dependency
                    var lastState = await sensorService.GetById(clientId, condition.DeviceId);

                    if (lastState != null) conditionMet = EvaluateCondition(condition, lastState.Value, lastState.Type);
                }

                // Linking the results
                if (rule.LogicalOperator == LogicalOperator.All)
                {
                    isTriggered &= conditionMet;
                    if (!isTriggered) break; // AND: Once false, all false -> break
                }
                else
                {
                    isTriggered |= conditionMet;
                    if (isTriggered) break; // OR: Once true, all true -> break
                }
            }

            if (isTriggered)
            {
                rule.LastTriggered = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                foreach (var action in rule.Actions)
                    await action.ExecuteAsync(_serviceProvider, clientId, payload, rule);
            }
        }
    }

    private bool EvaluateCondition(RuleCondition condition, string value, SensorType type)
    {
        var strategy = _strategies.FirstOrDefault(s => s.SupportedType == type);
        if (strategy == null) return false;

        return strategy.Evaluate(value, condition.Operator, condition.ThresholdValue, condition.IgnoreCase);
    }
}