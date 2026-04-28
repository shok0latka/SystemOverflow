#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Arithmetic
{
    public sealed class ModIntInt : ModuloOperator
    {
        protected override object? EvaluateImpl(Expression left, Expression right)
        {
            var a = Convert.ToInt32(left.Evaluate());
            var b = Convert.ToInt32(right.Evaluate());
            return ((a % b) + b) % b;
        }

        public ModIntInt()
        {
            LeftArg = ScriptType.Integer;
            RightArg = ScriptType.Integer;
            ResultType = ScriptType.Integer;
        }
    }
}
