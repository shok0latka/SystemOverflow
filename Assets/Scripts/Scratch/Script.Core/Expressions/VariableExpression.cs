using Script.Core.Variables;

namespace Script.Core.Expressions;

public class VariableExpression: Expression
{
    public Variable Var { get; init; }

    public override object? Evaluate()
    {
        return Var.Raw;
    }

    public VariableExpression(Variable v)
    {
        Var = v;
        Type = Var.Type; // Возможно будут проблемы с обновлением типа
    }
}
