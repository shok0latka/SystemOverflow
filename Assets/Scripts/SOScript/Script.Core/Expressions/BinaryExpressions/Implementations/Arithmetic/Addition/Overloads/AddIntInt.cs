#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Addition.Overloads
{
    public sealed class AddIntInt : AdditionOperator
    {
        protected override object? EvaluateImpl(Expression left, Expression right)
        {
            return Convert.ToInt32(left.Evaluate()) + Convert.ToInt32(right.Evaluate());
        }

        public AddIntInt()
        {
            LeftArg = ScriptType.Integer;
            RightArg = ScriptType.Integer;
            ResultType = ScriptType.Integer;
        }
    }
}