using IoTBroker.Models;
using IoTBroker.Rules.Models;

namespace IoTBroker.Rules.Helper;

/// <summary>
///     Utility class for replacing tokens in templates with actual values from sensor payloads and rules.
/// </summary>
public static class TokenReplacer
{
    /// <summary>
    ///     Replaces tokens in the given template with values from the provided sensor payload and rule.
    /// </summary>
    /// <param name="template">The template string containing tokens to be replaced.</param>
    /// <param name="payload">The sensor payload providing values for replacement.</param>
    /// <param name="rule">The sensor rule providing additional context for replacement.</param>
    /// <returns>The template string with tokens replaced by actual values.</returns>
    public static string Replace(string? template, SensorPayload payload, SensorRule rule)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;

        var result = template
            .Replace("{rule.name}", rule.Name)
            .Replace("{rule.id}", rule.Id)
            .Replace("{device}", payload.DeviceId)
            .Replace("{value}", payload.Value)
            .Replace("{value.type}", payload.Type.ToString())
            .Replace("{timestamp}", DateTime.UtcNow.ToString("O"));

        var conditionsSummary = string.Join(", ", rule.Conditions.Select(c => $"{c.DeviceId} {c.Operator} {c.ThresholdValue}"));
        return result.Replace("{rule.conditions}", conditionsSummary);
    }
}