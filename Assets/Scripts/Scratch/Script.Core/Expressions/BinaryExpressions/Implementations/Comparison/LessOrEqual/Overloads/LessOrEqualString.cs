#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.LessOrEqual.Overloads
{
    public sealed class LessOrEqualString : LessOrEqualOperator
    {
        public LessOrEqualString()
        {
            LeftArg = ScriptType.String;
            RightArg = ScriptType.String;
        }

        protected override object? EvaluateImpl(Expression left, Expression right)
        {
            return string.CompareOrdinal(
                Convert.ToString(left.Evaluate()),
                Convert.ToString(right.Evaluate())
            ) <= 0;
        }
    }
}