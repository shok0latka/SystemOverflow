using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHackController : MonoBehaviour, IHackable
{
    private const float MinimumVisibleProgress = 0f;
    private const float MinimumHackDuration = 0.2f;
    private const float MinimumCommandStepDuration = 0.05f;
    private const int MinimumQueuedCommands = 1;

    [SerializeField] private EnemyHackProgressIndicator progressIndicator;
    [SerializeField] private EnemyHackCooldownIndicator cooldownIndicator;
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0f, 1.75f, 0f);
    [SerializeField] private float defaultDuration = 40f;
    [SerializeField] private int maxQueuedCommands = 24;
    [SerializeField] private float commandStepDuration = 0.35f;

    private EnemyAI2D _owner;
    private bool _isActive;
    private float _activeDuration;
    private float _timeRemaining;
    private bool _hasAttemptProgress;
    private float _attemptProgress;
    private readonly Queue<HackQueuedCommand> _queuedCommands = new();

    public int QueuedCommandCount => _queuedCommands.Count;
    public int MaxQueuedCommands => maxQueuedCommands;
    public float CommandStepDuration => commandStepDuration;
    public static EnemyHackController ActiveHack { get; private set; }

    private void Awake()
    {
        NormalizeConfig();
        EnsureIndicators(allowCreate: true);
    }

    private void Update()
    {
        TickActiveHack(Time.deltaTime);
    }

    private void LateUpdate()
    {
        RefreshPresentation();
    }

    private void OnValidate()
    {
        NormalizeConfig();
        if (Application.isPlaying)
        {
            EnsureIndicators(allowCreate: false);
        }
    }

    private void OnDisable()
    {
        if (_isActive || ActiveHack == this)
        {
            EndActiveHack();
        }
    }

    private void OnDestroy()
    {
        if (_isActive || ActiveHack == this)
        {
            EndActiveHack();
        }
    }

    public HackStatusSnapshot GetHackStatus()
    {
        bool canBegin = CanBeginHack();
        bool isAttempting = !_isActive && canBegin && _hasAttemptProgress;
        HackPhase phase = _isActive
            ? HackPhase.Active
            : isAttempting
                ? HackPhase.Attempting
                : HackPhase.Idle;
        float duration = _isActive
            ? _activeDuration
            : defaultDuration;
        float timeRemaining = _isActive
            ? _timeRemaining
            : 0f;
        float attemptProgress = isAttempting ? _attemptProgress : MinimumVisibleProgress;

        return new HackStatusSnapshot(
            phase,
            canBegin,
            _isActive,
            attemptProgress,
            duration,
            timeRemaining);
    }

    public HackBeginResult TryBeginHack(HackRequest request)
    {
        if (_isActive)
        {
            return HackBeginResult.Failure(HackFailureReason.AlreadyActive);
        }

        if (ResolveOwner() == null)
        {
            return HackBeginResult.Failure(HackFailureReason.MissingTarget);
        }

        if (!CanBeginHack())
        {
            return HackBeginResult.Failure(HackFailureReason.NotHackable);
        }

        float duration = request.Duration > 0f
            ? request.Duration
            : defaultDuration;
        ReplaceCurrentActiveHack();
        StartActiveHack(duration, duration);
        return HackBeginResult.Success(duration);
    }

    public bool TryCancelHack()
    {
        if (!_isActive)
        {
            return false;
        }

        EndActiveHack();
        return true;
    }

    public bool RestoreActiveHack(float duration, float timeRemaining)
    {
        if (duration <= 0f || timeRemaining <= 0f)
        {
            EndActiveHack();
            return false;
        }

        float safeDuration = Mathf.Max(MinimumHackDuration, duration, timeRemaining);
        ReplaceCurrentActiveHack();
        StartActiveHack(safeDuration, timeRemaining);
        return true;
    }

    public void SetAttemptProgress(float normalizedProgress)
    {
        HackStatusSnapshot status = GetHackStatus();
        if (!status.CanBegin)
        {
            ClearAttemptProgress();
            return;
        }

        _hasAttemptProgress = true;
        _attemptProgress = Mathf.Clamp01(normalizedProgress);
    }

    public void ClearAttemptProgress()
    {
        _hasAttemptProgress = false;
        _attemptProgress = 0f;
    }

    public bool TryEnqueueCommand(HackCommand command)
    {
        return TryEnqueueCommand(command, HackQueuedCommand.DefaultMovementDistance);
    }

    public bool TryEnqueueCommand(HackCommand command, float distance)
    {
        return TryEnqueueCommand(new HackQueuedCommand(command, distance));
    }

    public bool TryEnqueueCommand(HackQueuedCommand command)
    {
        if (!CanAcceptCommands() || command.Command == HackCommand.None)
        {
            return false;
        }

        if (_queuedCommands.Count >= maxQueuedCommands || !IsValidQueuedCommand(command))
        {
            return false;
        }

        _queuedCommands.Enqueue(command);
        return true;
    }

    public bool TryDequeueCommand(out HackQueuedCommand command)
    {
        if (!CanAcceptCommands())
        {
            ClearCommands();
            command = HackQueuedCommand.None;
            return false;
        }

        if (_queuedCommands.Count == 0)
        {
            command = HackQueuedCommand.None;
            return false;
        }

        command = _queuedCommands.Dequeue();
        return true;
    }

    public bool TryDequeueCommand(out HackCommand command)
    {
        if (TryDequeueCommand(out HackQueuedCommand queuedCommand))
        {
            command = queuedCommand.Command;
            return true;
        }

        command = HackCommand.None;
        return false;
    }

    public void ClearCommands()
    {
        _queuedCommands.Clear();
    }

    private void RefreshPresentation()
    {
        HackStatusSnapshot status = GetHackStatus();

        if (progressIndicator != null)
        {
            if (status.Phase == HackPhase.Attempting)
            {
                progressIndicator.ShowProgress(status.AttemptProgress);
            }
            else
            {
                progressIndicator.HideProgress();
            }

            progressIndicator.RefreshPresentation();
        }

        if (cooldownIndicator != null)
        {
            if (status.IsActive && status.NormalizedRemaining > 0f)
            {
                cooldownIndicator.ShowCooldown(status.NormalizedRemaining);
            }
            else
            {
                cooldownIndicator.HideCooldown();
            }

            cooldownIndicator.RefreshPresentation();
        }
    }

    private void StartActiveHack(float duration, float timeRemaining)
    {
        ActiveHack = this;
        _isActive = true;
        _activeDuration = Mathf.Max(MinimumHackDuration, duration);
        _timeRemaining = Mathf.Clamp(timeRemaining, 0f, _activeDuration);
        ClearAttemptProgress();
        ClearCommands();
    }

    private void EndActiveHack()
    {
        _isActive = false;
        _activeDuration = 0f;
        _timeRemaining = 0f;
        ClearAttemptProgress();
        ClearCommands();

        if (ActiveHack == this)
        {
            ActiveHack = null;
        }
    }

    private void ReplaceCurrentActiveHack()
    {
        if (ActiveHack == null || ActiveHack == this)
        {
            return;
        }

        ActiveHack.TryCancelHack();
    }

    private void TickActiveHack(float deltaTime)
    {
        if (!_isActive)
        {
            return;
        }

        _timeRemaining = Mathf.Max(0f, _timeRemaining - Mathf.Max(0f, deltaTime));
        if (_timeRemaining <= 0f)
        {
            EndActiveHack();
        }
    }

    private void EnsureIndicators(bool allowCreate)
    {
        progressIndicator = EnsureProgressIndicator(allowCreate);
        progressIndicator?.Configure(indicatorOffset, allowCreate);

        cooldownIndicator = EnsureCooldownIndicator(allowCreate);
        cooldownIndicator?.Configure(indicatorOffset, allowCreate);
    }

    private EnemyHackProgressIndicator EnsureProgressIndicator(bool allowCreate)
    {
        if (progressIndicator != null)
        {
            return progressIndicator;
        }

        progressIndicator = GetComponent<EnemyHackProgressIndicator>();
        if (progressIndicator == null && allowCreate)
        {
            progressIndicator = gameObject.AddComponent<EnemyHackProgressIndicator>();
        }

        return progressIndicator;
    }

    private EnemyHackCooldownIndicator EnsureCooldownIndicator(bool allowCreate)
    {
        if (cooldownIndicator != null)
        {
            return cooldownIndicator;
        }

        cooldownIndicator = GetComponent<EnemyHackCooldownIndicator>();
        if (cooldownIndicator == null && allowCreate)
        {
            cooldownIndicator = gameObject.AddComponent<EnemyHackCooldownIndicator>();
        }

        return cooldownIndicator;
    }

    private bool CanAcceptCommands()
    {
        return _isActive;
    }

    private bool IsValidQueuedCommand(HackQueuedCommand command)
    {
        if (command.Command == HackCommand.None)
        {
            return false;
        }

        return !command.IsMovement ||
            HackQueuedCommand.IsValidMovementDistance(command.Distance);
    }

    private bool CanBeginHack()
    {
        if (_isActive)
        {
            return false;
        }

        EnemyAI2D resolvedOwner = ResolveOwner();
        if (resolvedOwner == null)
        {
            return false;
        }

        return resolvedOwner.CurrentState switch
        {
            EnemyState.Patrol => true,
            EnemyState.Chase => true,
            EnemyState.Search => true,
            _ => false
        };
    }

    private void NormalizeConfig()
    {
        defaultDuration = Mathf.Max(MinimumHackDuration, defaultDuration);
        maxQueuedCommands = Mathf.Max(MinimumQueuedCommands, maxQueuedCommands);
        commandStepDuration = Mathf.Max(MinimumCommandStepDuration, commandStepDuration);
    }

    private EnemyAI2D ResolveOwner()
    {
        if (_owner != null)
        {
            return _owner;
        }

        _owner = GetComponent<EnemyAI2D>();
        return _owner;
    }
}
