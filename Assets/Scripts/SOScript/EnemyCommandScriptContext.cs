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
        if (!HasTarget)
        {
            throw new System.InvalidOperationException("No active hacked enemy is bound.");
        }

        if (!Target.TryEnqueueCommand(command))
        {
            Debug.LogWarning($"Failed to enqueue hack command '{command}'.", Target);
            throw new System.InvalidOperationException($"Could not enqueue command '{command}'.");
        }
    }
}
