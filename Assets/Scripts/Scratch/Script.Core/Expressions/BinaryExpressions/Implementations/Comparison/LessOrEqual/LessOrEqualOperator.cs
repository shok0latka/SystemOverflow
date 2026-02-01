using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.LessOrEqual
{
    public abstract class LessOrEqualOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public static BinaryOperatorTag Tag => BinaryOperatorTag.LessOrEqual;

        protected LessOrEqualOperator()
        {
            ResultType = ScriptType.Boolean;
        }
    }
}