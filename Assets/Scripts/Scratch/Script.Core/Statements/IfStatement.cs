using Script.Core.Expressions;
using Script.Core.Statements.ControlFlow;
using Script.Core.Types;

namespace Script.Core.Statements;

public sealed class IfStatement : IStatement
{
    private Expression? condition;
    public Expression? Condition
    {
        get => condition;
        set
        {
            if ((value?.Type ?? ScriptType.Undefined) is not ScriptType.Undefined or ScriptType.Boolean)
            {
                throw new ArgumentException("Incorrect condition type (expected bool or undefined)", nameof(Condition));
            }
            condition = value;
        }
    }

    public SequenceStatement? Then { get; set; }
    public SequenceStatement? Else { get; set; }

    public ControlFlowResult Execute()
    {
        if (Condition?.Type is not ScriptType.Boolean)
        {
            throw new ArgumentException("At runtime condition type must be bool", nameof(Condition));
        }

        if (Convert.ToBoolean(Condition?.Evaluate()))
        {
            return Then?.Execute() ?? ControlFlowResult.None;
        }
        else
        {
            return Else?.Execute() ?? ControlFlowResult.None;
        }
    }
}
