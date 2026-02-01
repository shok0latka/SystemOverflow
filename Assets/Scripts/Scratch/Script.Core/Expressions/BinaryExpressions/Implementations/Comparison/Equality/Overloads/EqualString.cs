using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.Equality.Overloads
{
    public sealed class EqualString : EqualityOperator
    {
        public EqualString()
        {
            LeftArg = ScriptType.String;
            RightArg = ScriptType.String;
        }

        protected override object? EvaluateImpl(Expression left, Expression right)
        {
            return string.Equals(
                Convert.ToString(left.Evaluate()),
                Convert.ToString(right.Evaluate()),
                StringComparison.Ordinal
            );
        }
    }
}