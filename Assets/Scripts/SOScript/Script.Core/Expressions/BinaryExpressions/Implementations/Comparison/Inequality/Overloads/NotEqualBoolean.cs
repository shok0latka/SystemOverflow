#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Comparison
{
    public sealed class NotEqualBoolean : NotEqualOperator
    {
        public NotEqualBoolean()
        {
            LeftArg = ScriptType.Boolean;
            RightArg = ScriptType.Boolean;
        }

        protected override object? EvaluateImpl(Expression left, Expression right)
        {
            return Convert.ToBoolean(left.Evaluate()) != Convert.ToBoolean(right.Evaluate());
        }
    }
}
