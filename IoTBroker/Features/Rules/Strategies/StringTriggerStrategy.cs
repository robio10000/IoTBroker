using IoTBroker.Domain;
using IoTBroker.Features.Rules.Models;

namespace IoTBroker.Features.Rules.Strategies;

/// <summary>
///     Trigger strategy for string sensor types.
/// </summary>
public class StringTriggerStrategy : ITriggerStrategy
{
    public SensorType SupportedType => SensorType.String;

    /// <summary>
    ///     Evaluates the sensor value against the threshold using the specified comparison operator.
    /// </summary>
    /// <param name="sensorValue">The sensor value as a string.</param>
    /// <param name="op">The comparison operator.</param>
    /// <param name="thresholdValue">The threshold value as a string.</param>
    /// <param name="ignoreCase">Indicates whether to ignore case in string comparisons.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    public bool Evaluate(string sensorValue, ComparisonOperator op, string thresholdValue, bool ignoreCase)
    {
        if (sensorValue == null || thresholdValue == null) return false;

        var comparison = ignoreCase 
            ? StringComparison.OrdinalIgnoreCase 
            : StringComparison.Ordinal;

        return op switch
        {
            ComparisonOperator.Equals => sensorValue.Equals(thresholdValue, comparison),
            ComparisonOperator.NotEquals => !sensorValue.Equals(thresholdValue, comparison),
            ComparisonOperator.Contains => sensorValue.Contains(thresholdValue, comparison),
            ComparisonOperator.StartsWith => sensorValue.StartsWith(thresholdValue, comparison),
            ComparisonOperator.EndsWith => sensorValue.EndsWith(thresholdValue, comparison),
            _ => false
        };
    }
}