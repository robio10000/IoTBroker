using IoTBroker.Domain;
using IoTBroker.Features.Rules.Models;

namespace IoTBroker.Features.Rules.Strategies;

/// <summary>
///     Trigger strategy for numeric sensor types.
/// </summary>
public class NumericTriggerStrategy : ITriggerStrategy
{
    public SensorType SupportedType => SensorType.Numeric;

    /// <summary>
    ///     Evaluates the sensor value against the threshold using the specified comparison operator.<br/>
    ///     They compare absolute without tolerance, so the user should be aware of their device precision.
    /// </summary>
    /// <param name="sensorValue">The sensor value as a string.</param>
    /// <param name="op">The comparison operator.</param>
    /// <param name="thresholdValue">The threshold value as a string.</param>
    /// <param name="ignoreCase">Unused here.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    public bool Evaluate(string sensorValue, ComparisonOperator op, string thresholdValue, bool ignoreCase = false)
    {
        if (!double.TryParse(sensorValue, out var sVal) || !double.TryParse(thresholdValue, out var tVal))
            return false;

        return op switch
        {
            ComparisonOperator.GreaterThan => sVal > tVal,
            ComparisonOperator.LessThan => sVal < tVal,
            ComparisonOperator.Equals => sVal == tVal,
            ComparisonOperator.NotEquals => sVal != tVal,
            _ => false
        };
    }
}