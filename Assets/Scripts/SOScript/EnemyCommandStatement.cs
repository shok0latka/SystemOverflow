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
        if (TryCollectEnemyCommandChain(out List<EnemyCommandStatement> commandStatements))
        {
            EnemyCommandScriptContext.EnqueueCommands(BuildQueuedCommands(commandStatements));
            return ControlFlowResult.None;
        }

        EnemyCommandScriptContext.EnqueueCommand(BuildQueuedCommand());
        return Next?.Execute() ?? ControlFlowResult.None;
    }

    public async Task<ControlFlowResult> ExecuteAsync()
    {
        if (TryCollectEnemyCommandChain(out List<EnemyCommandStatement> commandStatements))
        {
            List<HackQueuedCommand> queuedCommands = await BuildQueuedCommandsAsync(commandStatements);
            foreach (EnemyCommandStatement statement in commandStatements)
            {
                await statement.InvokeExecutePulseAsync();
            }

            EnemyCommandScriptContext.EnqueueCommands(queuedCommands);
            return ControlFlowResult.None;
        }

        await InvokeExecutePulseAsync();
        EnemyCommandScriptContext.EnqueueCommand(await BuildQueuedCommandAsync());
        return await (Next?.ExecuteAsync() ?? Task.FromResult(ControlFlowResult.None));
    }

    private bool TryCollectEnemyCommandChain(out List<EnemyCommandStatement> commandStatements)
    {
        commandStatements = new List<EnemyCommandStatement> { this };

        IStatement? current = Next;
        while (current != null)
        {
            if (current is not EnemyCommandStatement enemyCommandStatement)
            {
                commandStatements.Clear();
                return false;
            }

            commandStatements.Add(enemyCommandStatement);
            current = enemyCommandStatement.Next;
        }

        return true;
    }

    private static List<HackQueuedCommand> BuildQueuedCommands(List<EnemyCommandStatement> commandStatements)
    {
        var queuedCommands = new List<HackQueuedCommand>(commandStatements.Count);
        foreach (EnemyCommandStatement statement in commandStatements)
        {
            queuedCommands.Add(statement.BuildQueuedCommand());
        }

        return queuedCommands;
    }

    private static async Task<List<HackQueuedCommand>> BuildQueuedCommandsAsync(List<EnemyCommandStatement> commandStatements)
    {
        var queuedCommands = new List<HackQueuedCommand>(commandStatements.Count);
        foreach (EnemyCommandStatement statement in commandStatements)
        {
            queuedCommands.Add(await statement.BuildQueuedCommandAsync());
        }

        return queuedCommands;
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
            HackCommand.MoveForward => "Move Forward",
            HackCommand.MoveLeft => "Move Left",
            HackCommand.MoveRight => "Move Right",
            HackCommand.MoveGlobalUp => "Move Global Up",
            HackCommand.MoveGlobalDown => "Move Global Down",
            HackCommand.MoveGlobalLeft => "Move Global Left",
            HackCommand.MoveGlobalRight => "Move Global Right",
            HackCommand.RotateLeft => "Rotate Left",
            HackCommand.RotateRight => "Rotate Right",
            HackCommand.Interact => "Interact",
            _ => "Enemy Command"
        };
    }
}
