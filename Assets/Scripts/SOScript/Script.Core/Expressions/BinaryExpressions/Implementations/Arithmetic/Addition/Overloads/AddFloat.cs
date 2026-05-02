#nullable enable

using System;
using System.Collections.Generic;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Arithmetic
{
    public sealed class AddFloat : AdditionOperator, ISelfRegistrableOverload
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
        {
            return Convert.ToSingle(left.Evaluate()) + Convert.ToSingle(right.Evaluate());
        }

        public void Register(Dictionary<(ScriptType, ScriptType), BinaryOperatorOverload> overloads)
        {
            var instance = new AddFloat();
            List<(ScriptType, ScriptType)> keys = new() {
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

        public AddFloat()
        {
            LeftArg = ScriptType.Float;
            RightArg = ScriptType.Float;
            ResultType = ScriptType.Float;
        }
    }
}
