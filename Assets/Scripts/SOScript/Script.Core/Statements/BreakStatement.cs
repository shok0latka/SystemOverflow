#nullable enable

using System;
using Script.Core.Statements.ControlFlow;

namespace Script.Core.Statements
{
    public class BreakStatement : IStatement
    {
        public ControlFlowResult Execute()
        {
            return ControlFlowResult.Break;
        }
    }
}