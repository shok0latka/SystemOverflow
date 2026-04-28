#nullable enable

using System;
using System.Linq;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Arithmetic
{
    public sealed class MulStringInt : MultiplicationOperator
    {
        protected override object? EvaluateImpl(Expression left, Expression right)
        {
            var str = Convert.ToString(left.Evaluate())!;
            var n = Convert.ToInt32(right.Evaluate());
            if (n <= 0) return string.Empty;
            return string.Concat(Enumerable.Repeat(str, n));
        }

        public MulStringInt()
        {
            LeftArg = ScriptType.String;
            RightArg = ScriptType.Integer;
            ResultType = ScriptType.String;
        }
    }
}
