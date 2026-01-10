using IoTBroker.Domain;
using IoTBroker.Features.Rules.Models;

namespace IoTBroker.Features.Rules.Strategies;

/// <summary>
///     Trigger strategy for boolean sensor types.
/// </summary>
public class BooleanTriggerStrategy : ITriggerStrategy
{
    public SensorType SupportedType => SensorType.Boolean;

    /// <summary>
    ///     Evaluates the sensor value against the threshold using the specified comparison operator.
    /// </summary>
    /// <param name="sensorValue">The sensor value as a string.</param>
    /// <param name="op">The comparison operator.</param>
    /// <param name="thresholdValue">The threshold value as a string.</param>
    /// <param name="ignoreCase">Unused here.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    public bool Evaluate(string sensorValue, ComparisonOperator op, string thresholdValue, bool ignoreCase)
    {
        if (!bool.TryParse(sensorValue, out var sVal) || !bool.TryParse(thresholdValue, out var tVal))
            return false;

        return op switch
        {
            ComparisonOperator.Equals => sVal == tVal,
            ComparisonOperator.NotEquals => sVal != tVal,
            _ => false
        };
    }
}