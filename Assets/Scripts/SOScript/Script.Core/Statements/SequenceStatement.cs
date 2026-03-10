#nullable enable

using System;
using System.Collections.Generic;
using Script.Core.Statements.ControlFlow;

namespace Script.Core.Statements
{
    public sealed class SequenceStatement : IStatement
    {
        private readonly List<IStatement> statements = new ();

        public IReadOnlyList<IStatement> Statements => statements;

        public void Insert(int index, IStatement statement)
        {
            statements.Insert(index, statement);
        }

        public void Add(IStatement statement)
        {
            statements.Add(statement);
        }

        public void RemoveAt(int index)
        {
            statements.RemoveAt(index);
        }

        public ControlFlowResult Execute()
        {
            foreach (var stmt in statements)
            {
                var result = stmt.Execute();
                if (result.Kind != ControlFlowKind.None)
                    return result;
            }
            return ControlFlowResult.None;
        }
    }
}