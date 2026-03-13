#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Script.Core.Statements.ControlFlow;

namespace Script.Core.Statements
{
    public class BreakStatement : IStatement
    {
        public event Func<Task>? OnExecuteAsync;

        public List<StatementArgument> Arguments { get; } = new();
        public IStatement? Next { get; set; }

        IReadOnlyList<StatementArgument> IStatement.Arguments => Arguments;

        public ControlFlowResult Execute()
        {
            return ControlFlowResult.Break;
        }

        public async Task<ControlFlowResult> ExecuteAsync()
        {
            if (OnExecuteAsync != null)
                await OnExecuteAsync();

            return ControlFlowResult.Break;
        }
    }
}