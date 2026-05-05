#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Script.Core.Statements;
using Script.Core.Statements.ControlFlow;
using Script.Core.Types;

public sealed class EnemyCommandStatement : IStatement
{
    public EnemyCommandStatement(HackCommand command)
    {
        Command = command;
        if (HackQueuedCommand.IsMovementCommand(command))
        {
            Arguments.Add(new StatementArgument(
                "Distance",
                new List<ScriptType> { ScriptType.Integer, ScriptType.Float }));
        }
    }

    public event Func<Task>? OnExecuteAsync;

    public HackCommand Command { get; }
    public List<StatementArgument> Arguments { get; } = new();
    public IStatement? Next { get; set; }
    IReadOnlyList<StatementArgument> IStatement.Arguments => Arguments;
    public string Name => GetDisplayName(Command);

    public ControlFlowResult Execute()
    {
        EnemyCommandScriptContext.EnqueueCommand(BuildQueuedCommand());
        return Next?.Execute() ?? ControlFlowResult.None;
    }

    public async Task<ControlFlowResult> ExecuteAsync()
    {
        await InvokeExecutePulseAsync();
        EnemyCommandScriptContext.EnqueueCommand(await BuildQueuedCommandAsync());
        return await (Next?.ExecuteAsync() ?? Task.FromResult(ControlFlowResult.None));
    }

    private HackQueuedCommand BuildQueuedCommand()
    {
        if (!HackQueuedCommand.IsMovementCommand(Command))
        {
            return new HackQueuedCommand(Command, 0f);
        }

        object? distanceValue = Arguments[0].Evaluate();
        return new HackQueuedCommand(Command, ConvertDistance(distanceValue));
    }

    private async Task<HackQueuedCommand> BuildQueuedCommandAsync()
    {
        if (!HackQueuedCommand.IsMovementCommand(Command))
        {
            return new HackQueuedCommand(Command, 0f);
        }

        object? distanceValue = await Arguments[0].EvaluateAsync();
        return new HackQueuedCommand(Command, ConvertDistance(distanceValue));
    }

    private async Task InvokeExecutePulseAsync()
    {
        if (OnExecuteAsync != null)
        {
            await OnExecuteAsync();
        }
    }

    private static float ConvertDistance(object? value)
    {
        float distance = value switch
        {
            int intValue => intValue,
            float floatValue => floatValue,
            double doubleValue => (float)doubleValue,
            _ => throw new ArgumentException("Distance must be a number.")
        };

        if (!HackQueuedCommand.IsValidMovementDistance(distance))
        {
            throw new ArgumentException("Distance must be a finite number greater than 0.");
        }

        return distance;
    }

    public static string GetDisplayName(HackCommand command)
    {
        return command switch
        {
            HackCommand.MoveUp => "Move Up",
            HackCommand.MoveDown => "Move Down",
            HackCommand.MoveLeft => "Move Left",
            HackCommand.MoveRight => "Move Right",
            HackCommand.RotateCounterClockwise => "Rotate Counterclockwise",
            HackCommand.RotateClockwise => "Rotate Clockwise",
            HackCommand.Interact => "Interact",
            _ => "Enemy Command"
        };
    }
}
