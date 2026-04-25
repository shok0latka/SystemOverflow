using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHackController : MonoBehaviour, IHackable, IHackCommandSink
{
    private const float MinimumVisibleProgress = 0f;
    private const float MinimumHackDuration = 0.2f;

    [SerializeField] private EnemyHackProgressIndicator progressIndicator;
    [SerializeField] private EnemyHackCooldownIndicator cooldownIndicator;
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0f, 1.75f, 0f);
    [SerializeField] private float defaultDuration = 40f;

    private EnemyAI2D _owner;
    private bool _isActive;
    private float _activeDuration;
    private float _timeRemaining;
    private bool _hasAttemptProgress;
    private float _attemptProgress;
    private bool _hasControlIntent;
    private HackControlIntent _controlIntent;
    private bool _interactRequested;

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

    public bool TrySetControlIntent(HackControlIntent intent)
    {
        if (!CanAcceptCommands())
        {
            return false;
        }

        _controlIntent = HackControlIntent.Clamp(intent);
        _hasControlIntent = true;
        return true;
    }

    public bool TryRequestInteract()
    {
        if (!CanAcceptCommands())
        {
            return false;
        }

        _interactRequested = true;
        return true;
    }

    public void ClearControlIntent()
    {
        bool hasStoredCommand = _hasControlIntent ||
            _interactRequested ||
            HasAnyInput(_controlIntent);
        if (!hasStoredCommand)
        {
            return;
        }

        _hasControlIntent = false;
        _controlIntent = default;
        _interactRequested = false;
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
        _isActive = true;
        _activeDuration = Mathf.Max(MinimumHackDuration, duration);
        _timeRemaining = Mathf.Clamp(timeRemaining, 0f, _activeDuration);
        ClearAttemptProgress();
        ClearControlIntent();
    }

    private void EndActiveHack()
    {
        _isActive = false;
        _activeDuration = 0f;
        _timeRemaining = 0f;
        ClearAttemptProgress();
        ClearControlIntent();
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

    private static bool HasAnyInput(HackControlIntent intent)
    {
        return !Mathf.Approximately(intent.MoveRight, 0f) ||
            !Mathf.Approximately(intent.MoveForward, 0f) ||
            !Mathf.Approximately(intent.Turn, 0f);
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
            EnemyState.ReturnToPatrol => true,
            _ => false
        };
    }

    private void NormalizeConfig()
    {
        defaultDuration = Mathf.Max(MinimumHackDuration, defaultDuration);
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
