#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Arithmetic
{
    public sealed class SubIntInt : SubtractionOperator
    {
        protected override object? EvaluateImpl(Expression left, Expression right)
            => Convert.ToInt32(left.Evaluate()) - Convert.ToInt32(right.Evaluate());

        public SubIntInt()
        {
            LeftArg = ScriptType.Integer;
            RightArg = ScriptType.Integer;
            ResultType = ScriptType.Integer;
        }
    }
}
