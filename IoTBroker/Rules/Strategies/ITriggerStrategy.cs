using IoTBroker.Models;
using IoTBroker.Rules.Models;

namespace IoTBroker.Rules.Strategies;

/// <summary>
///     Together with the Strategy Pattern, this interface defines how different trigger strategies should behave.
/// </summary>
public interface ITriggerStrategy
{
    // What type from your core enum does this strategy support?
    // e.g., SensorType.Numeric, SensorType.Boolean, SensorType.String, etc.
    SensorType SupportedType { get; }

    /// <summary>
    ///     Evaluates the sensor value against the threshold value using the specified comparison operator.
    /// </summary>
    /// <param name="sensorValue">The current value from the sensor as a string.</param>
    /// <param name="op">The comparison operator to use for evaluation.</param>
    /// <param name="thresholdValue">The threshold value to compare against as a string.</param>
    /// <param name="ignoreCase">Indicates whether to ignore case in string comparisons. Default is false.</param>
    /// <returns>True if the condition is met, otherwise false.</returns>
    bool Evaluate(string sensorValue, ComparisonOperator op, string thresholdValue, bool ignoreCase = false);
}