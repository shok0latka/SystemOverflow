using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions;

public abstract class BinaryOperatorOverload
{
    private ScriptType leftArg;
    private ScriptType rightArg;
    private ScriptType resultType;

    public ScriptType LeftArg
    {
        get => leftArg;
        init => leftArg = 
            value != ScriptType.Undefined ? 
            value : 
            throw new ArgumentException(
                "Left argument type must be defined", 
                nameof(LeftArg)
            );
    }

    public ScriptType RightArg
    {
        get => rightArg;
        init => rightArg = 
            value != ScriptType.Undefined ? 
            value : 
            throw new ArgumentException(
                "Right argument type must be defined", 
                nameof(RightArg)
            );
    }

    public ScriptType ResultType
    {
        get => resultType;
        init => resultType = 
            value != ScriptType.Undefined ? 
            value : 
            throw new ArgumentException(
                "Result type must be defined", 
                nameof(ResultType)
            );
    }

    public object? Evaluate(Expression left, Expression right)
    {
        ValidateType(left.Type, right.Type);
        return EvaluateImpl(left, right);
    }

    protected virtual void ValidateType(ScriptType left, ScriptType right)
    {
        if (left != LeftArg)
        {
            throw new ArgumentException($"Left argument type mismatch. Expected: {LeftArg}. Got {left}");
        }

        if (right != RightArg)
        {
            throw new ArgumentException($"Right argument type mismatch. Expected: {RightArg}. Got {right}");
        }
    }

    protected abstract object? EvaluateImpl(Expression left, Expression right);
}
