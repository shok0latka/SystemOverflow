using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.Equality.Overloads;

public sealed class EqualNumeric : EqualityOperator, ISelfRegistrableOverload
{
    public EqualNumeric()
    {
        LeftArg = ScriptType.Float;
        RightArg = ScriptType.Float;
    }

    protected override void ValidateType(ScriptType left, ScriptType right)
    {
        if (!(left is ScriptType.Float or ScriptType.Integer))
        {
            throw new ArgumentException($"Left argument type mismatch. Expected: {ScriptType.Float} or {ScriptType.Integer}. Got {left}");
        }
        if (!(right is ScriptType.Float or ScriptType.Integer))
        {
            throw new ArgumentException($"Right argument type mismatch. Expected: {ScriptType.Float} or {ScriptType.Integer}. Got {right}");
        }
    }

    public static void Register(ref Dictionary<(ScriptType, ScriptType), BinaryOperatorOverload> overloads)
    {
        var instance = new EqualNumeric();
        List<(ScriptType, ScriptType)> keys = [
            (ScriptType.Float, ScriptType.Float),
            (ScriptType.Integer, ScriptType.Float),
            (ScriptType.Float, ScriptType.Integer),
            (ScriptType.Integer, ScriptType.Integer)
        ];

        foreach (var key in keys)
        {
            if (!overloads.TryAdd(key, instance))
            {
                throw new InvalidOperationException(
                    $"Duplicate overload found for {Tag} with key {key}");
            }
        }
    }
    
    protected override object? EvaluateImpl(Expression left, Expression right)
    {
        return Math.Abs(Convert.ToSingle(left.Evaluate()) - Convert.ToSingle(right.Evaluate())) < 1e-6;
    }
}
