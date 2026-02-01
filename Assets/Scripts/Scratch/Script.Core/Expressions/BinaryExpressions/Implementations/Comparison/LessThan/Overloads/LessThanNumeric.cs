using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.LessThan.Overloads;

public sealed class LessThanNumeric : LessThanOperator, ISelfRegistrableOverload
{
    public LessThanNumeric()
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
        return Convert.ToSingle(left.Evaluate()) <
               Convert.ToSingle(right.Evaluate());
    }

    public static void Register(ref Dictionary<(ScriptType, ScriptType), BinaryOperatorOverload> overloads)
    {
        var instance = new LessThanNumeric();
        var keys = new[]
        {
            (ScriptType.Integer, ScriptType.Integer),
            (ScriptType.Integer, ScriptType.Float),
            (ScriptType.Float, ScriptType.Integer),
            (ScriptType.Float, ScriptType.Float),
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
