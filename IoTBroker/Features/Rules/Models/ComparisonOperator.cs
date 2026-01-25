namespace IoTBroker.Features.Rules.Models;

/// <summary>
///     Comparison operators for rule evaluations.
/// </summary>
public enum ComparisonOperator
{
    GreaterThan,
    LessThan,
    Equals,
    NotEquals,
    Contains,
    StartsWith,
    EndsWith
}