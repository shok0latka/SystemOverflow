using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Addition.Overloads;

public sealed class AddFloat : AdditionOperator, ISelfRegistrableOverload
{
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

    protected override object? EvaluateImpl(Expression left, Expression right)
    {
        return Convert.ToSingle(left.Evaluate()) + Convert.ToSingle(right.Evaluate());
    }

    public static void Register(ref Dictionary<(ScriptType, ScriptType), BinaryOperatorOverload> overloads)
    {
        var instance = new AddFloat();
        List<(ScriptType, ScriptType)> keys = [
            (ScriptType.Float, ScriptType.Float),
            (ScriptType.Integer, ScriptType.Float),
            (ScriptType.Float, ScriptType.Integer)
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

    public AddFloat()
    {
        LeftArg = ScriptType.Float;
        RightArg = ScriptType.Float;
        ResultType = ScriptType.Float;
    }
}
