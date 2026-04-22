using System;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyAI2D : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string persistentId;

    [Header("Config")]
    [FormerlySerializedAs("archetype")]
    [SerializeField] private EnemyConfig enemyConfig;

    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform player;

    [Header("Perception")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Status Indicator")]
    [SerializeField] private TextMesh statusText;
    [SerializeField] private Vector3 statusOffset = new Vector3(0f, 1.4f, 0f);

    [Header("Debug Runtime")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrol;
    [SerializeField, Range(0f, 1f)] private float suspicion;

    private EnemyContext _context;
    private EnemyHackController _hackController;
    private EnemyHackProgressIndicator _hackProgressIndicator;
    private EnemyStatusIndicator _statusIndicator;
    private EnemyVisionOutline _visionOutline;
    private EnemyStateMachine _stateMachine;

    private EnemyConfig _boundConfig;
    private Transform[] _boundPatrolPoints;
    private Transform _boundPlayer;
    private int _boundObstacleMask;
    private bool _bindingsDirty = true;

    public string SaveId => persistentId;
    public EnemyState CurrentState => _stateMachine?.CurrentState ?? currentState;
    public bool IsHackActive => CurrentState == EnemyState.Hacked && (_context?.HackedTimer ?? 0f) > 0f;
    public bool CanBeHacked => CurrentState switch
    {
        EnemyState.Patrol => true,
        EnemyState.Chase => true,
        EnemyState.ReturnToPatrol => true,
        _ => false
    };

    private void Awake()
    {
        EnsurePersistentId(ensureUniqueInScene: false);

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (!ValidateDependencies())
        {
            return;
        }

        ConfigureHackController(allowCreate: true);
        ConfigureHackProgressIndicator(allowCreate: true);
        ConfigureStatusIndicator(allowCreate: true);
        ConfigureVisionOutline(allowCreate: true);
        InitializeRuntime();
        ApplyContextBindings(force: true);
        SyncDebugRuntime();
    }

    private void FixedUpdate()
    {
        if (_context == null || _stateMachine == null)
        {
            return;
        }

        ApplyContextBindings(force: false);
        UpdateViewDirectionForCurrentState(_stateMachine.CurrentState);
        _context.TickCooldowns(Time.fixedDeltaTime);
        _context.TickPerception(Time.fixedDeltaTime, _stateMachine.CurrentState != EnemyState.Hacked);

        _stateMachine.TickUpdate(Time.fixedDeltaTime);
        _stateMachine.TickFixed(Time.fixedDeltaTime);
        SyncDebugRuntime();
    }

    private void LateUpdate()
    {
        _hackProgressIndicator?.RefreshPresentation();
        _statusIndicator?.RefreshPresentation();
        RefreshVisionOutline(allowCreate: true);
    }

    private void OnValidate()
    {
        EnsurePersistentId(ensureUniqueInScene: !Application.isPlaying);

        if (!Application.isPlaying)
        {
            return;
        }

        _bindingsDirty = true;
        ConfigureHackController(allowCreate: false);
        ConfigureHackProgressIndicator(allowCreate: false);
        ConfigureStatusIndicator(allowCreate: false);
        ConfigureVisionOutline(allowCreate: false);
        ApplyContextBindings(force: false);
        RefreshVisionOutline(allowCreate: false);
    }

    private void OnDestroy()
    {
        if (_stateMachine != null)
        {
            _stateMachine.StateChanged -= HandleStateChanged;
        }
    }

    public EnemyRuntimeSaveData CaptureRuntimeState()
    {
        EnemyState state = _stateMachine?.CurrentState ?? EnemyState.Patrol;
        Vector2 position = _context?.Position ?? (Vector2)transform.position;
        Vector2 lastKnown = _context?.LastKnownPlayerPosition ?? position;

        return new EnemyRuntimeSaveData
        {
            saveId = SaveId,
            posX = position.x,
            posY = position.y,
            state = state.ToString(),
            patrolIndex = _context?.PatrolIndex ?? 0,
            suspicion = _context?.Suspicion.Value ?? 0f,
            timeSinceSeen = _context?.TimeSinceSeenPlayer ?? 0f,
            attackTimer = _context?.AttackCooldownTimer ?? 0f,
            searchTimer = _context?.ReturnTimer ?? 0f,
            hackedTimer = _context?.HackedTimer ?? 0f,
            lastKnownX = lastKnown.x,
            lastKnownY = lastKnown.y
        };
    }

    public void RestoreRuntimeState(EnemyRuntimeSaveData data)
    {
        if (data == null)
        {
            return;
        }

        if (_context == null || _stateMachine == null)
        {
            if (!ValidateDependencies())
            {
                return;
            }

            InitializeRuntime();
            ApplyContextBindings(force: true);
        }

        _context.Position = new Vector2(data.posX, data.posY);
        _context.PatrolIndex = Mathf.Max(0, data.patrolIndex);
        _context.Suspicion.Set(data.suspicion);
        _context.TimeSinceSeenPlayer = Mathf.Max(0f, data.timeSinceSeen);
        _context.AttackCooldownTimer = Mathf.Max(0f, data.attackTimer);
        _context.ReturnTimer = Mathf.Max(0f, data.searchTimer);
        _context.HackedTimer = Mathf.Max(0f, data.hackedTimer);
        _context.LastKnownPlayerPosition = new Vector2(data.lastKnownX, data.lastKnownY);

        EnemyState restoredState = ParseState(data.state);
        if (restoredState == EnemyState.Hacked && _context.HackedTimer <= 0f)
        {
            _context.HackedTimer = 0.2f;
        }

        _stateMachine.TransitionTo(restoredState);
        UpdateViewDirectionForCurrentState(_stateMachine.CurrentState);
        SyncDebugRuntime();
    }

    private bool ValidateDependencies()
    {
        if (rb == null)
        {
            Debug.LogError($"[{nameof(EnemyAI2D)}] Missing Rigidbody2D on '{name}'. Component disabled.", this);
            enabled = false;
            return false;
        }

        if (enemyConfig == null)
        {
            Debug.LogError($"[{nameof(EnemyAI2D)}] Missing EnemyConfig on '{name}'. Component disabled.", this);
            enabled = false;
            return false;
        }

        return true;
    }

    private void InitializeRuntime()
    {
        if (_stateMachine != null)
        {
            _stateMachine.StateChanged -= HandleStateChanged;
        }

        _context = new EnemyContext(this, enemyConfig, rb, patrolPoints, player, obstacleMask);
        _context.SetHackController(_hackController);
        _context.EnsurePlayerReference();

        if (player == null && _context.Player != null)
        {
            player = _context.Player;
        }

        if (_context.Player != null)
        {
            _context.LastKnownPlayerPosition = _context.Player.position;
        }

        _stateMachine = new EnemyStateMachine();
        _stateMachine.StateChanged += HandleStateChanged;
        _stateMachine.Register(new PatrolState(_context, _stateMachine));
        _stateMachine.Register(new ChaseState(_context, _stateMachine));
        _stateMachine.Register(new AttackState(_context, _stateMachine));
        _stateMachine.Register(new HackedState(_context, _stateMachine));
        _stateMachine.Register(new ReturnToPatrolState(_context, _stateMachine));
        _stateMachine.Initialize(EnemyState.Patrol);
        UpdateViewDirectionForCurrentState(_stateMachine.CurrentState);
    }

    private void ApplyContextBindings(bool force)
    {
        if (_context == null)
        {
            return;
        }

        bool configChanged = _boundConfig != enemyConfig;
        bool patrolChanged = !ReferenceEquals(_boundPatrolPoints, patrolPoints);
        bool playerChanged = _boundPlayer != player;
        bool obstacleChanged = _boundObstacleMask != obstacleMask.value;
        bool shouldApply = force || _bindingsDirty || configChanged || patrolChanged || playerChanged || obstacleChanged;

        if (!shouldApply)
        {
            return;
        }

        if (enemyConfig != null)
        {
            _context.SetConfig(enemyConfig);
        }

        _context.SetPatrolPoints(patrolPoints);
        _context.ObstacleMask = obstacleMask;
        _context.SetHackController(_hackController);

        if (playerChanged || force || _bindingsDirty)
        {
            _context.SetPlayer(player);
        }

        _context.EnsurePlayerReference();
        if (player == null && _context.Player != null)
        {
            player = _context.Player;
        }

        _boundConfig = enemyConfig;
        _boundPatrolPoints = patrolPoints;
        _boundPlayer = player;
        _boundObstacleMask = obstacleMask.value;
        _bindingsDirty = false;
    }

    private void HandleStateChanged(EnemyState _, EnemyState toState)
    {
        currentState = toState;
        _hackProgressIndicator?.HideProgress();
        _statusIndicator?.ApplyState(currentState);
    }

    private void SyncDebugRuntime()
    {
        if (_context != null)
        {
            suspicion = _context.Suspicion.Value;
        }
    }

    private void ConfigureStatusIndicator(bool allowCreate)
    {
        _statusIndicator = EnsureStatusIndicatorComponent(allowCreate);
        if (_statusIndicator == null)
        {
            return;
        }

        _statusIndicator.Configure(statusText, statusOffset, allowCreate);
        _statusIndicator.ApplyState(currentState, allowCreate);
    }

    private void ConfigureHackController(bool allowCreate)
    {
        _hackController = EnsureHackControllerComponent(allowCreate);
        if (_hackController != null)
        {
            _hackController.BindOwner(this);
        }

        if (_context != null)
        {
            _context.SetHackController(_hackController);
        }
    }

    private void ConfigureHackProgressIndicator(bool allowCreate)
    {
        _hackProgressIndicator = EnsureHackProgressIndicatorComponent(allowCreate);
        if (_hackProgressIndicator == null)
        {
            return;
        }

        _hackProgressIndicator.Configure(statusOffset + new Vector3(0f, 0.35f, 0f), allowCreate);
    }

    private void ConfigureVisionOutline(bool allowCreate)
    {
        _visionOutline = EnsureVisionOutlineComponent(allowCreate);
    }

    private EnemyStatusIndicator EnsureStatusIndicatorComponent(bool allowCreate)
    {
        if (_statusIndicator != null)
        {
            return _statusIndicator;
        }

        _statusIndicator = GetComponent<EnemyStatusIndicator>();
        if (_statusIndicator == null)
        {
            if (!allowCreate)
            {
                return null;
            }

            _statusIndicator = gameObject.AddComponent<EnemyStatusIndicator>();
        }

        return _statusIndicator;
    }

    private EnemyHackProgressIndicator EnsureHackProgressIndicatorComponent(bool allowCreate)
    {
        if (_hackProgressIndicator != null)
        {
            return _hackProgressIndicator;
        }

        _hackProgressIndicator = GetComponent<EnemyHackProgressIndicator>();
        if (_hackProgressIndicator == null)
        {
            if (!allowCreate)
            {
                return null;
            }

            _hackProgressIndicator = gameObject.AddComponent<EnemyHackProgressIndicator>();
        }

        return _hackProgressIndicator;
    }

    private EnemyVisionOutline EnsureVisionOutlineComponent(bool allowCreate)
    {
        if (_visionOutline != null)
        {
            return _visionOutline;
        }

        _visionOutline = GetComponent<EnemyVisionOutline>();
        if (_visionOutline == null)
        {
            if (!allowCreate)
            {
                return null;
            }

            _visionOutline = gameObject.AddComponent<EnemyVisionOutline>();
        }

        return _visionOutline;
    }

    private EnemyHackController EnsureHackControllerComponent(bool allowCreate)
    {
        if (_hackController != null)
        {
            return _hackController;
        }

        _hackController = GetComponent<EnemyHackController>();
        if (_hackController == null)
        {
            if (!allowCreate)
            {
                return null;
            }

            _hackController = gameObject.AddComponent<EnemyHackController>();
        }

        return _hackController;
    }

    public bool TryBeginHack(float baseDuration)
    {
        if (_context == null || _stateMachine == null)
        {
            if (!ValidateDependencies())
            {
                return false;
            }

            InitializeRuntime();
            ApplyContextBindings(force: true);
        }

        if (!CanBeHacked || IsHackActive)
        {
            return false;
        }

        _context.StartHack(baseDuration);
        _stateMachine.TransitionTo(EnemyState.Hacked);
        return true;
    }

    public void ShowHackProgress(float normalizedProgress)
    {
        if (!CanBeHacked)
        {
            HideHackProgress();
            return;
        }

        if (_hackProgressIndicator == null)
        {
            ConfigureHackProgressIndicator(allowCreate: true);
        }

        _hackProgressIndicator?.ShowProgress(normalizedProgress);
    }

    public void HideHackProgress()
    {
        _hackProgressIndicator?.HideProgress();
    }

    private void RefreshVisionOutline(bool allowCreate)
    {
        _visionOutline = EnsureVisionOutlineComponent(allowCreate);
        if (_visionOutline == null)
        {
            return;
        }

        Vector3 origin = _context != null ? (Vector3)_context.Position : transform.position;
        Vector2 viewDirection = _context != null ? _context.ViewDirection : Vector2.right;
        float radius = enemyConfig != null ? Mathf.Max(0f, enemyConfig.visionRadius) : 0f;
        float closeVisionRadius = _context != null
            ? _context.CloseVisionRadius
            : enemyConfig != null
                ? Mathf.Max(1f, enemyConfig.attackRadius)
                : 1f;
        float coneAngleDegrees = _context != null
            ? _context.ViewConeAngleDegrees
            : enemyConfig != null
                ? Mathf.Clamp(enemyConfig.visionConeAngleDegrees, 1f, 360f)
                : 90f;
        bool shouldShow = _context != null && currentState != EnemyState.Hacked;

        _visionOutline.RefreshOutline(
            origin,
            viewDirection,
            radius,
            closeVisionRadius,
            coneAngleDegrees,
            shouldShow,
            allowCreate);
    }

    private void UpdateViewDirectionForCurrentState(EnemyState state)
    {
        if (_context == null)
        {
            return;
        }

        switch (state)
        {
            case EnemyState.Patrol:
                if (TryGetPatrolFacingTarget(out Vector2 patrolTarget))
                {
                    _context.UpdateViewDirectionTowards(patrolTarget);
                }
                break;
            case EnemyState.Hacked:
                break;
            case EnemyState.Chase:
                _context.UpdateViewDirectionTowards(_context.LastKnownPlayerPosition);
                break;
            case EnemyState.Attack:
                if (_context.Player != null)
                {
                    _context.UpdateViewDirectionTowards(_context.Player.position);
                }
                break;
            case EnemyState.ReturnToPatrol:
                _context.UpdateViewDirectionTowards(_context.LastKnownPlayerPosition);
                break;
        }
    }

    private bool TryGetPatrolFacingTarget(out Vector2 patrolTarget)
    {
        patrolTarget = default;

        Transform[] points = _context?.PatrolPoints;
        if (points == null || points.Length == 0)
        {
            return false;
        }

        int patrolIndex = _context.PatrolIndex;
        if (patrolIndex < 0 || patrolIndex >= points.Length)
        {
            patrolIndex = 0;
        }

        Transform currentPatrolPoint = points[patrolIndex];
        if (currentPatrolPoint == null)
        {
            return false;
        }

        patrolTarget = currentPatrolPoint.position;
        if (_context.IsNear(patrolTarget, 0.2f))
        {
            Transform nextPatrolPoint = points[(patrolIndex + 1) % points.Length];
            if (nextPatrolPoint != null)
            {
                patrolTarget = nextPatrolPoint.position;
            }
        }

        return true;
    }

    private static EnemyState ParseState(string rawState)
    {
        if (!string.IsNullOrEmpty(rawState) &&
            Enum.TryParse(rawState, true, out EnemyState parsedState))
        {
            return parsedState;
        }

        return EnemyState.Patrol;
    }

    private void EnsurePersistentId(bool ensureUniqueInScene)
    {
        if (string.IsNullOrWhiteSpace(persistentId))
        {
            persistentId = Guid.NewGuid().ToString("N");
        }

        if (!ensureUniqueInScene)
        {
            return;
        }

        EnemyAI2D[] allEnemies = FindObjectsByType<EnemyAI2D>(FindObjectsSortMode.None);
        foreach (EnemyAI2D enemy in allEnemies)
        {
            if (enemy == this)
            {
                continue;
            }

            if (enemy.persistentId == persistentId)
            {
                persistentId = Guid.NewGuid().ToString("N");
                break;
            }
        }
    }
}
