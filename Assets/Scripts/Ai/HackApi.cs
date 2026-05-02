using UnityEngine;

public interface IHackable
{
    HackStatusSnapshot GetHackStatus();
    HackBeginResult TryBeginHack(HackRequest request);
    bool TryCancelHack();
    void SetAttemptProgress(float normalizedProgress);
    void ClearAttemptProgress();
}

public enum HackPhase
{
    Idle,
    Attempting,
    Active
}

public enum HackCommand
{
    None,
    MoveForward,
    MoveLeft,
    MoveRight,
    RotateLeft,
    RotateRight,
    Interact,
    MoveGlobalUp,
    MoveGlobalDown,
    MoveGlobalLeft,
    MoveGlobalRight
}

public readonly struct HackQueuedCommand
{
    public const float DefaultMovementDistance = 1f;

    public HackQueuedCommand(HackCommand command, float distance = DefaultMovementDistance)
    {
        Command = command;
        Distance = distance;
    }

    public HackCommand Command { get; }
    public float Distance { get; }

    public static HackQueuedCommand None => new HackQueuedCommand(HackCommand.None, 0f);

    public bool IsMovement => IsMovementCommand(Command);

    public static bool IsMovementCommand(HackCommand command)
    {
        return command switch
        {
            HackCommand.MoveForward => true,
            HackCommand.MoveLeft => true,
            HackCommand.MoveRight => true,
            HackCommand.MoveGlobalUp => true,
            HackCommand.MoveGlobalDown => true,
            HackCommand.MoveGlobalLeft => true,
            HackCommand.MoveGlobalRight => true,
            _ => false
        };
    }

    public static bool IsValidMovementDistance(float distance)
    {
        return distance > 0f && !float.IsNaN(distance) && !float.IsInfinity(distance);
    }
}

public enum HackFailureReason
{
    None,
    MissingTarget,
    NotHackable,
    AlreadyActive
}

public readonly struct HackRequest
{
    public HackRequest(float duration)
    {
        Duration = Mathf.Max(0f, duration);
    }

    public static HackRequest Default => new HackRequest(0f);
    public float Duration { get; }
}

public readonly struct HackStatusSnapshot
{
    public HackStatusSnapshot(
        HackPhase phase,
        bool canBegin,
        bool isActive,
        float attemptProgress,
        float duration,
        float timeRemaining)
    {
        Phase = phase;
        CanBegin = canBegin;
        IsActive = isActive;
        AttemptProgress = Mathf.Clamp01(attemptProgress);
        Duration = Mathf.Max(0f, duration);
        TimeRemaining = Mathf.Clamp(timeRemaining, 0f, Duration);
        NormalizedRemaining = Duration > 0f
            ? Mathf.Clamp01(TimeRemaining / Duration)
            : 0f;
    }

    public static HackStatusSnapshot Unavailable => new HackStatusSnapshot(
        HackPhase.Idle,
        canBegin: false,
        isActive: false,
        attemptProgress: 0f,
        duration: 0f,
        timeRemaining: 0f);

    public HackPhase Phase { get; }
    public bool CanBegin { get; }
    public bool IsActive { get; }
    public float AttemptProgress { get; }
    public float Duration { get; }
    public float TimeRemaining { get; }
    public float NormalizedRemaining { get; }
}

public readonly struct HackBeginResult
{
    private HackBeginResult(bool succeeded, HackFailureReason failureReason, float duration)
    {
        Succeeded = succeeded;
        FailureReason = failureReason;
        Duration = Mathf.Max(0f, duration);
    }

    public bool Succeeded { get; }
    public HackFailureReason FailureReason { get; }
    public float Duration { get; }

    public static HackBeginResult Success(float duration)
    {
        return new HackBeginResult(true, HackFailureReason.None, duration);
    }

    public static HackBeginResult Failure(HackFailureReason failureReason)
    {
        return new HackBeginResult(false, failureReason, 0f);
    }
}
