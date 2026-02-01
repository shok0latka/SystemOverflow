using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.Equality.Overloads
{
    public sealed class EqualBoolean : EqualityOperator
    {
        public EqualBoolean()
        {
            LeftArg = ScriptType.Boolean;
            RightArg = ScriptType.Boolean;
        }

        protected override object? EvaluateImpl(Expression left, Expression right)
        {
            return Convert.ToBoolean(left.Evaluate()) == Convert.ToBoolean(right.Evaluate());
        }
    }
}