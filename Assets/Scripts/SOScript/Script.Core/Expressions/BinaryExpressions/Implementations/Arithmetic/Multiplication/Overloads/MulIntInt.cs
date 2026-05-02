#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Arithmetic
{
    public sealed class MulIntInt : MultiplicationOperator
    {
        protected override object? EvaluateImpl(Expression left, Expression right)
            => Convert.ToInt32(left.Evaluate()) * Convert.ToInt32(right.Evaluate());

        public MulIntInt()
        {
            LeftArg = ScriptType.Integer;
            RightArg = ScriptType.Integer;
            ResultType = ScriptType.Integer;
        }
    }
}
