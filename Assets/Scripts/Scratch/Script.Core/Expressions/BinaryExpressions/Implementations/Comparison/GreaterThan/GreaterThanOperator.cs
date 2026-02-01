using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.GreaterThan;

public abstract class GreaterThanOperator : BinaryOperatorOverload, ITaggedBinaryOperator
{
    public static BinaryOperatorTag Tag => BinaryOperatorTag.GreaterThan;

    protected GreaterThanOperator()
    {
        ResultType = ScriptType.Boolean;
    }
}
