using UnityEngine;

public interface IHackable
{
    HackStatusSnapshot GetHackStatus();
    HackBeginResult TryBeginHack(HackRequest request);
    bool TryCancelHack();
    void SetAttemptProgress(float normalizedProgress);
    void ClearAttemptProgress();
}

public interface IHackCommandSink
{
    bool TrySetControlIntent(HackControlIntent intent);
    bool TryRequestInteract();
    void ClearControlIntent();
}

public enum HackPhase
{
    Idle,
    Attempting,
    Active
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

public readonly struct HackControlIntent
{
    public HackControlIntent(float moveRight, float moveForward, float turn)
    {
        MoveRight = Mathf.Clamp(moveRight, -1f, 1f);
        MoveForward = Mathf.Clamp(moveForward, -1f, 1f);
        Turn = Mathf.Clamp(turn, -1f, 1f);
    }

    public float MoveRight { get; }
    public float MoveForward { get; }
    public float Turn { get; }

    public static HackControlIntent Clamp(HackControlIntent intent)
    {
        return new HackControlIntent(intent.MoveRight, intent.MoveForward, intent.Turn);
    }
}
