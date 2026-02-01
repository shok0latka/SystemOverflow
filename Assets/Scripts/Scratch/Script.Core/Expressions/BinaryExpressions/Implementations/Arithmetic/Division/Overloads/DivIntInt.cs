using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Division.Overloads
{
    public sealed class DivIntInt : DivisionOperator
    {
        protected override object? EvaluateImpl(Expression left, Expression right)
        {
            var a = Convert.ToInt32(left.Evaluate());
            var b = Convert.ToInt32(right.Evaluate());
            return a / b;
        }

        public DivIntInt()
        {
            LeftArg = ScriptType.Integer;
            RightArg = ScriptType.Integer;
            ResultType = ScriptType.Integer;
        }
    }
}