#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Script.Core.Statements;
using Script.Core.Statements.ControlFlow;

public sealed class EnemyCommandStatement : IStatement
{
    public EnemyCommandStatement(HackCommand command)
    {
        Command = command;
    }

    public event Func<Task>? OnExecuteAsync;

    public HackCommand Command { get; }
    public List<StatementArgument> Arguments { get; } = new();
    public IStatement? Next { get; set; }
    IReadOnlyList<StatementArgument> IStatement.Arguments => Arguments;
    public string Name => GetDisplayName(Command);

    public ControlFlowResult Execute()
    {
        EnemyCommandScriptContext.EnqueueCommand(Command);
        return Next?.Execute() ?? ControlFlowResult.None;
    }

    public async Task<ControlFlowResult> ExecuteAsync()
    {
        if (OnExecuteAsync != null)
        {
            await OnExecuteAsync();
        }

        EnemyCommandScriptContext.EnqueueCommand(Command);
        return await (Next?.ExecuteAsync() ?? Task.FromResult(ControlFlowResult.None));
    }

    public static string GetDisplayName(HackCommand command)
    {
        return command switch
        {
            HackCommand.MoveForward => "Move Forward",
            HackCommand.MoveLeft => "Move Left",
            HackCommand.MoveRight => "Move Right",
            HackCommand.RotateLeft => "Rotate Left",
            HackCommand.RotateRight => "Rotate Right",
            HackCommand.Interact => "Interact",
            _ => "Enemy Command"
        };
    }
}
