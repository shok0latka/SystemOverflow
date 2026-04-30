#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Script.Core.Statements;
using Script.Core.Statements.ControlFlow;

public sealed class ProgramRootStatement : IStatement
{
    public event Func<Task>? OnExecuteAsync;

    public IStatement? Body { get; set; }
    public IStatement? Next { get; set; }
    public IReadOnlyList<StatementArgument> Arguments { get; } = new List<StatementArgument>();
    public string Name => "Program";

    public ControlFlowResult Execute()
    {
        ControlFlowResult result = Body?.Execute() ?? ControlFlowResult.None;
        if (result.Kind != ControlFlowKind.None)
        {
            return result;
        }

        return Next?.Execute() ?? ControlFlowResult.None;
    }

    public async Task<ControlFlowResult> ExecuteAsync()
    {
        if (OnExecuteAsync != null)
        {
            await OnExecuteAsync();
        }

        ControlFlowResult result = await (Body?.ExecuteAsync() ?? Task.FromResult(ControlFlowResult.None));
        if (result.Kind != ControlFlowKind.None)
        {
            return result;
        }

        return await (Next?.ExecuteAsync() ?? Task.FromResult(ControlFlowResult.None));
    }
}
