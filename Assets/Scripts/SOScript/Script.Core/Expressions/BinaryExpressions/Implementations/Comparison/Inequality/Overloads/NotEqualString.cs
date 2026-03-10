#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.Inequality.Overloads
{
    public sealed class NotEqualString : NotEqualOperator
    {
        public NotEqualString()
        {
            LeftArg = ScriptType.String;
            RightArg = ScriptType.String;
        }

        protected override object? EvaluateImpl(Expression left, Expression right)
        {
            return !string.Equals(
                Convert.ToString(left.Evaluate()),
                Convert.ToString(right.Evaluate()),
                StringComparison.Ordinal
            );
        }
    }
}