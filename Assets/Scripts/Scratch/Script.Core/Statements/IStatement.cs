using Script.Core.Statements.ControlFlow;

namespace Script.Core.Statements;

public interface IStatement
{
    ControlFlowResult Execute();
}
