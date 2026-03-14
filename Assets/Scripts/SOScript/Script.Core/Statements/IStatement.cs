#nullable enable

using System;
using Script.Core.Statements.ControlFlow;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Script.Core.Statements
{
    public interface IStatement
    {
        ControlFlowResult Execute();

        Task<ControlFlowResult> ExecuteAsync();

        event Func<Task>? OnExecuteAsync;

        IStatement? Next { get; set; }
        IReadOnlyList<StatementArgument> Arguments { get; }
        string Name { get; }
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

        public static IStatement RegisterExecutionCallback(this IStatement statement, Func<Task> callback)
        {
            statement.OnExecuteAsync += callback;
            return statement;
        }
    }
}

