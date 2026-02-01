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
            var newType = value?.Type ?? ScriptType.Undefined;
            if (newType != ScriptType.Undefined && newType != ScriptType.Boolean)
            {
                throw new ArgumentException($"Incorrect condition type {newType}. Expected: {ScriptType.Boolean} or {ScriptType.Undefined}", nameof(Condition));
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
