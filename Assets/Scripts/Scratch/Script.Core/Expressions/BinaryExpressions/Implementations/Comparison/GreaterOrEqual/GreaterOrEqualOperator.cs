using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.GreaterOrEqual;

public abstract class GreaterOrEqualOperator : BinaryOperatorOverload, ITaggedBinaryOperator
{
    public static BinaryOperatorTag Tag => BinaryOperatorTag.GreaterOrEqual;

    protected GreaterOrEqualOperator()
    {
        ResultType = ScriptType.Boolean;
    }
}
