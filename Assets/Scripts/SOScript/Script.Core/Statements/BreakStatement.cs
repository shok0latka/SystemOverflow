#nullable enable

using System;
using System.Collections.Generic;
using Script.Core.Statements.ControlFlow;

namespace Script.Core.Statements
{
    public class BreakStatement : IStatement
    {
        public List<StatementArgument> Arguments { get; } = new();
        public IStatement? Next { get; set; }

        IReadOnlyList<StatementArgument> IStatement.Arguments => Arguments;

        public ControlFlowResult Execute()
        {
            return ControlFlowResult.Break;
        }
    }
}