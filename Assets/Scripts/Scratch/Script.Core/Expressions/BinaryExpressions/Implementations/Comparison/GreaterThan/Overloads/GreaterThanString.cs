using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.GreaterThan.Overloads;

public sealed class GreaterThanString : GreaterThanOperator
{
    public GreaterThanString()
    {
        LeftArg = ScriptType.String;
        RightArg = ScriptType.String;
    }

    protected override object? EvaluateImpl(Expression left, Expression right)
    {
        return string.CompareOrdinal(
            Convert.ToString(left.Evaluate()),
            Convert.ToString(right.Evaluate())
        ) > 0;
    }
}
