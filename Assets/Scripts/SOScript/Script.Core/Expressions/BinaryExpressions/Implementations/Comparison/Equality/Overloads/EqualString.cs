#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Comparison
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
