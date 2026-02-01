using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.LessThan;

public abstract class LessThanOperator : BinaryOperatorOverload, ITaggedBinaryOperator
{
    public static BinaryOperatorTag Tag => BinaryOperatorTag.LessThan;

    protected LessThanOperator()
    {
        ResultType = ScriptType.Boolean;
    }
}
