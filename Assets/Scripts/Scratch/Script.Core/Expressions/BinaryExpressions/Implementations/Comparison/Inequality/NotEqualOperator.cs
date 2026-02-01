using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.Inequality
{
    public abstract class NotEqualOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public static BinaryOperatorTag Tag => BinaryOperatorTag.NotEqual;

        protected NotEqualOperator()
        {
            ResultType = ScriptType.Boolean;
        }
    }
}