using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.Equality
{
    public abstract class EqualityOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public static BinaryOperatorTag Tag => BinaryOperatorTag.Equal;

        protected EqualityOperator()
        {
            ResultType = ScriptType.Boolean;
        }
    }
}