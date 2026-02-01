using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Addition.Overloads
{
    public sealed class AddString : AdditionOperator
    {
        protected override object? EvaluateImpl(Expression left, Expression right)
            => Convert.ToString(left.Evaluate()) + Convert.ToString(right.Evaluate());

        public AddString()
        {
            LeftArg = ScriptType.String;
            RightArg = ScriptType.String;
            ResultType = ScriptType.String;
        }
    }
}