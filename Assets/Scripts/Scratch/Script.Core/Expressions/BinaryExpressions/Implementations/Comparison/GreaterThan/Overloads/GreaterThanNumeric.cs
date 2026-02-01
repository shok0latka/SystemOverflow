using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.GreaterThan.Overloads
{
    public sealed class GreaterThanNumeric : GreaterThanOperator, ISelfRegistrableOverload
    {
        public GreaterThanNumeric()
        {
            LeftArg = ScriptType.Float;
            RightArg = ScriptType.Float;
        }

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
            return Convert.ToSingle(left.Evaluate()) >
                Convert.ToSingle(right.Evaluate());
        }

        public static void Register(ref Dictionary<(ScriptType, ScriptType), BinaryOperatorOverload> overloads)
        {
            var instance = new GreaterThanNumeric();
            List<(ScriptType, ScriptType)> keys = new () { 
                (ScriptType.Float, ScriptType.Float),
                (ScriptType.Integer, ScriptType.Float),
                (ScriptType.Float, ScriptType.Integer),
                (ScriptType.Integer, ScriptType.Integer)
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
    }
}