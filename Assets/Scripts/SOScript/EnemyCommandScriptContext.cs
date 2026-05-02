using System.Collections.Generic;
using UnityEngine;

public static class EnemyCommandScriptContext
{
    public static EnemyHackController Target { get; private set; }

    public static bool HasTarget => Target != null && Target.GetHackStatus().IsActive;

    public static void Bind(EnemyHackController target)
    {
        Target = target;
    }

    public static void Clear(EnemyHackController expectedTarget = null)
    {
        if (expectedTarget != null && Target != expectedTarget)
        {
            return;
        }

        Target = null;
    }

    public static void ClearQueuedCommands()
    {
        Target?.ClearCommands();
    }

    public static void EnqueueCommand(HackCommand command)
    {
        EnqueueCommand(new HackQueuedCommand(command, HackQueuedCommand.DefaultMovementDistance));
    }

    public static void EnqueueCommand(HackCommand command, float distance)
    {
        EnqueueCommand(new HackQueuedCommand(command, distance));
    }

    public static void EnqueueCommand(HackQueuedCommand command)
    {
        if (!HasTarget)
        {
            throw new System.InvalidOperationException("No active hacked enemy is bound.");
        }

        if (!Target.TryEnqueueCommand(command))
        {
            Debug.LogWarning($"Failed to enqueue hack command '{command.Command}'.", Target);
            throw new System.InvalidOperationException($"Could not enqueue command '{command.Command}'.");
        }
    }

    public static void EnqueueCommands(IReadOnlyList<HackQueuedCommand> commands)
    {
        if (!HasTarget)
        {
            throw new System.InvalidOperationException("No active hacked enemy is bound.");
        }

        if (commands == null || commands.Count == 0)
        {
            throw new System.InvalidOperationException("No commands were provided.");
        }

        if (!Target.TryEnqueueCommands(commands))
        {
            Debug.LogWarning($"Failed to enqueue {commands.Count} hack commands.", Target);
            throw new System.InvalidOperationException("Could not enqueue command sequence.");
        }
    }
}
