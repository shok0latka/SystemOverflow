using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.GreaterOrEqual.Overloads;

public sealed class GreaterOrEqualString : GreaterOrEqualOperator
{
    public GreaterOrEqualString()
    {
        LeftArg = ScriptType.String;
        RightArg = ScriptType.String;
    }

    protected override object? EvaluateImpl(Expression left, Expression right)
    {
        return string.CompareOrdinal(
            Convert.ToString(left.Evaluate()),
            Convert.ToString(right.Evaluate())
        ) >= 0;
    }
}
