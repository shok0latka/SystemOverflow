#nullable enable

using System;
using Script.Core.Statements.ControlFlow;

using System.Collections.Generic;

namespace Script.Core.Statements
{
    public interface IStatement
    {
        ControlFlowResult Execute();
        IStatement? Next { get; set; }
        IReadOnlyList<StatementArgument> Arguments { get; }
    }

    public static class StatementExtensions
    {
        public static IStatement Then(this IStatement statement, IStatement next)
        {
            var current = statement;

            while (current.Next != null)
                current = current.Next;

            current.Next = next;

            return statement;
        }
    }
}

