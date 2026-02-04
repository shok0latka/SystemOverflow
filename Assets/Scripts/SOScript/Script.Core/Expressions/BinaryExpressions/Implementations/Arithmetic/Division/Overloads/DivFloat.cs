#nullable enable

using System;
using System.Collections.Generic;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Division.Overloads
{
    public sealed class DivFloat : DivisionOperator, ISelfRegistrableOverload
    {
        protected override void ValidateType(ScriptType left, ScriptType right)
        {
            if (!(left is ScriptType.Float or ScriptType.Integer))
            {
                throw new ArgumentException($"Left type mismatch: {left}");
            }
            if (!(right is ScriptType.Float or ScriptType.Integer))
            {
                throw new ArgumentException($"Right type mismatch: {right}");
            }
        }

        protected override object? EvaluateImpl(Expression left, Expression right)
            => Convert.ToSingle(left.Evaluate()) / Convert.ToSingle(right.Evaluate());

        public void Register(Dictionary<(ScriptType, ScriptType), BinaryOperatorOverload> overloads)
        {
            var instance = new DivFloat();
            var keys = new List<(ScriptType, ScriptType)>
            {
                (ScriptType.Float, ScriptType.Float),
                (ScriptType.Integer, ScriptType.Float),
                (ScriptType.Float, ScriptType.Integer)
            };
            foreach (var key in keys)
            {
                if (!overloads.TryAdd(key, instance))
                {
                    throw new InvalidOperationException(
                        $"Duplicate overload found for {Tag} with key {key}");
                }
            }
        }

        public DivFloat()
        {
            LeftArg = ScriptType.Float;
            RightArg = ScriptType.Float;
            ResultType = ScriptType.Float;
        }
    }
}