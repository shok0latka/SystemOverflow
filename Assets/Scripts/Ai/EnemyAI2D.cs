using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI2D : MonoBehaviour
{
    private const float DefaultBodyColliderRadius = 0.35f;

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

    [Header("Suspicion Indicator")]
    [SerializeField] private TextMesh suspicionText;
    [SerializeField] private Vector3 suspicionOffset = new Vector3(0f, 1.65f, 0f);

    [Header("Debug Indicators")]
    [SerializeField] private bool showEnemyDebugIndicators;

    [Header("Debug Runtime")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrol;
    [SerializeField, Range(0f, 1f)] private float suspicion;

    private EnemyContext _context;
    private EnemyHackController _hackController;
    private EnemyStatusIndicator _statusIndicator;
    private EnemySuspicionIndicator _suspicionIndicator;
    private EnemyVisionOutline _visionOutline;
    private EnemyStateMachine _stateMachine;

    private EnemyConfig _boundConfig;
    private Transform[] _boundPatrolPoints;
    private Transform _boundPlayer;
    private int _boundObstacleMask;
    private bool _bindingsDirty = true;
    private bool _hasSentAlertForCurrentChase;
    private bool _suppressAlertBroadcast;

    public string SaveId => persistentId;
    public EnemyState CurrentState => _stateMachine?.CurrentState ?? currentState;
    private float DefaultHackDuration => enemyConfig != null ? enemyConfig.hackDuration : 0f;

    private void Awake()
    {
        EnsurePersistentId(ensureUniqueInScene: false);
        ConfigureTopDownRigidbody();
        ConfigureBodyCollider(allowCreate: true);

        if (!ValidateDependencies())
        {
            return;
        }

        ConfigureHackController(allowCreate: true);
        if (showEnemyDebugIndicators)
        {
            ConfigureStatusIndicator(allowCreate: true);
            ConfigureVisionOutline(allowCreate: true);
        }

        ConfigureSuspicionIndicator(allowCreate: true);
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

        SyncHackStateFromController();
        _stateMachine.TickUpdate(Time.fixedDeltaTime);
        _stateMachine.TickFixed(Time.fixedDeltaTime);
        SyncDebugRuntime();
    }

    private void LateUpdate()
    {
        RefreshSuspicionIndicator(allowCreate: true);

        if (showEnemyDebugIndicators)
        {
            RefreshStatusIndicator(allowCreate: true);
            RefreshVisionOutline(allowCreate: true);
        }
    }

    private void OnValidate()
    {
        EnsurePersistentId(ensureUniqueInScene: !Application.isPlaying);
        ConfigureTopDownRigidbody();
        ConfigureBodyCollider(allowCreate: false);

        if (!Application.isPlaying)
        {
            return;
        }

        if (!CanRefreshRuntimeFromValidation())
        {
            return;
        }

        _bindingsDirty = true;
        ConfigureHackController(allowCreate: false);
        if (showEnemyDebugIndicators)
        {
            ConfigureStatusIndicator(allowCreate: false);
            ConfigureVisionOutline(allowCreate: false);
        }

        ConfigureSuspicionIndicator(allowCreate: false);
        ApplyContextBindings(force: false);
        RefreshSuspicionIndicator(allowCreate: false);

        if (showEnemyDebugIndicators)
        {
            RefreshStatusIndicator(allowCreate: false);
            RefreshVisionOutline(allowCreate: false);
        }
    }

    private void Reset()
    {
        ConfigureTopDownRigidbody();
        ConfigureBodyCollider(allowCreate: true);
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
        HackStatusSnapshot hackStatus = _hackController != null
            ? _hackController.GetHackStatus()
            : HackStatusSnapshot.Unavailable;
        EnemyState state = hackStatus.IsActive
            ? EnemyState.Hacked
            : _stateMachine?.CurrentState ?? EnemyState.Patrol;
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
            hackedTimer = hackStatus.IsActive ? hackStatus.TimeRemaining : 0f,
            hackDuration = hackStatus.IsActive && hackStatus.Duration > 0f
                ? hackStatus.Duration
                : DefaultHackDuration,
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
        _context.LastKnownPlayerPosition = new Vector2(data.lastKnownX, data.lastKnownY);

        EnemyState restoredState = ParseState(data.state);
        if (_hackController == null)
        {
            ConfigureHackController(allowCreate: true);
        }

        if (restoredState == EnemyState.Hacked)
        {
            float restoredDuration = data.hackDuration > 0f
                ? data.hackDuration
                : DefaultHackDuration;
            bool restoredHack = _hackController != null &&
                _hackController.RestoreActiveHack(restoredDuration, data.hackedTimer);
            if (!restoredHack)
            {
                restoredState = EnemyState.Patrol;
            }
        }
        else
        {
            _hackController?.TryCancelHack();
        }

        _suppressAlertBroadcast = true;
        _stateMachine.TransitionTo(restoredState);
        _suppressAlertBroadcast = false;
        UpdateViewDirectionForCurrentState(_stateMachine.CurrentState);
        SyncDebugRuntime();
    }

    public void ReceiveEnemyAlert(Vector2 playerPosition)
    {
        if (!CanReceiveEnemyAlert())
        {
            return;
        }

        if (_context == null || _stateMachine == null)
        {
            if (!ValidateDependencies())
            {
                return;
            }

            ConfigureHackController(allowCreate: true);
            InitializeRuntime();
            ApplyContextBindings(force: true);
        }

        float alertSuspicion = enemyConfig != null ? enemyConfig.alertSuspicion : 0.45f;
        _context.LastKnownPlayerPosition = playerPosition;
        _context.Suspicion.Set(Mathf.Max(_context.Suspicion.Value, alertSuspicion));
        _context.TimeSinceSeenPlayer = 0f;
        _context.ResetReturnTimer();
        _stateMachine.TransitionTo(EnemyState.Search);
        UpdateViewDirectionForCurrentState(_stateMachine.CurrentState);
        SyncDebugRuntime();
    }

    private bool ValidateDependencies()
    {
        ConfigureTopDownRigidbody();

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

    private bool CanRefreshRuntimeFromValidation()
    {
        ResolveRigidbodyReference();
        return rb != null && enemyConfig != null;
    }

    private void ResolveRigidbodyReference()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void ConfigureTopDownRigidbody()
    {
        ResolveRigidbodyReference();
        if (rb == null)
        {
            return;
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    private void ConfigureBodyCollider(bool allowCreate)
    {
        Collider2D bodyCollider = GetComponent<Collider2D>();
        if (bodyCollider == null)
        {
            if (!allowCreate)
            {
                return;
            }

            CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.radius = DefaultBodyColliderRadius;
            bodyCollider = circleCollider;
        }

        bodyCollider.isTrigger = false;
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
        _stateMachine.Register(new SearchState(_context, _stateMachine));
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

        _context.SetConfig(enemyConfig);
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

    private void HandleStateChanged(EnemyState fromState, EnemyState toState)
    {
        currentState = toState;
        _hackController?.ClearAttemptProgress();

        if (toState == EnemyState.Chase)
        {
            if (fromState != EnemyState.Chase &&
                !_hasSentAlertForCurrentChase &&
                !_suppressAlertBroadcast)
            {
                TrySendEnemyAlert();
                _hasSentAlertForCurrentChase = true;
            }
        }
        else if (toState == EnemyState.Patrol ||
            toState == EnemyState.Search ||
            toState == EnemyState.Hacked)
        {
            _hasSentAlertForCurrentChase = false;
        }

        if (showEnemyDebugIndicators)
        {
            _statusIndicator?.ApplyState(currentState);
        }
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

    private void RefreshStatusIndicator(bool allowCreate)
    {
        _statusIndicator = EnsureStatusIndicatorComponent(allowCreate);
        if (_statusIndicator == null)
        {
            return;
        }

        _statusIndicator.ApplyState(currentState, allowCreate);
        _statusIndicator.RefreshPresentation();
    }

    private void ConfigureSuspicionIndicator(bool allowCreate)
    {
        _suspicionIndicator = EnsureSuspicionIndicatorComponent(allowCreate);
        if (_suspicionIndicator == null)
        {
            return;
        }

        _suspicionIndicator.Configure(suspicionText, suspicionOffset, allowCreate);
    }

    private void ConfigureHackController(bool allowCreate)
    {
        _hackController = EnsureHackControllerComponent(allowCreate);

        if (_context != null)
        {
            _context.SetHackController(_hackController);
        }
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

    private EnemySuspicionIndicator EnsureSuspicionIndicatorComponent(bool allowCreate)
    {
        if (_suspicionIndicator != null)
        {
            return _suspicionIndicator;
        }

        _suspicionIndicator = GetComponent<EnemySuspicionIndicator>();
        if (_suspicionIndicator == null)
        {
            if (!allowCreate)
            {
                return null;
            }

            _suspicionIndicator = gameObject.AddComponent<EnemySuspicionIndicator>();
        }

        return _suspicionIndicator;
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

    private void SyncHackStateFromController()
    {
        if (_hackController == null || _stateMachine == null)
        {
            return;
        }

        HackStatusSnapshot hackStatus = _hackController.GetHackStatus();
        if (hackStatus.IsActive && _stateMachine.CurrentState != EnemyState.Hacked)
        {
            _stateMachine.TransitionTo(EnemyState.Hacked);
            return;
        }

        if (!hackStatus.IsActive && _stateMachine.CurrentState == EnemyState.Hacked)
        {
            _stateMachine.TransitionTo(EnemyState.Patrol);
        }
    }

    private void RefreshSuspicionIndicator(bool allowCreate)
    {
        _suspicionIndicator = EnsureSuspicionIndicatorComponent(allowCreate);
        if (_suspicionIndicator == null)
        {
            return;
        }

        float currentSuspicion = _context?.Suspicion.Value ?? suspicion;
        float suspicionThreshold = enemyConfig != null ? enemyConfig.suspicionThreshold : 1f;
        bool shouldShow = _context != null &&
            currentState != EnemyState.Hacked &&
            currentState != EnemyState.Chase &&
            currentState != EnemyState.Attack;

        _suspicionIndicator.RefreshSuspicion(
            currentSuspicion,
            suspicionThreshold,
            shouldShow,
            allowCreate);
        _suspicionIndicator.RefreshPresentation();
    }

    private void RefreshVisionOutline(bool allowCreate)
    {
        _visionOutline = EnsureVisionOutlineComponent(allowCreate);
        if (_visionOutline == null || _context == null)
        {
            return;
        }

        Vector3 origin = _context.Position;
        Vector2 viewDirection = _context.ViewDirection;
        bool shouldShow = currentState != EnemyState.Hacked;

        _visionOutline.RefreshOutline(
            origin,
            viewDirection,
            enemyConfig.visionRadius,
            _context.CloseVisionRadius,
            _context.ViewConeAngleDegrees,
            shouldShow,
            allowCreate);
    }

    private void TrySendEnemyAlert()
    {
        if (_context == null || enemyConfig == null || enemyConfig.alertRadius <= 0f)
        {
            return;
        }

        EnemyAI2D receiver = FindNearestAlertReceiver();
        if (receiver == null)
        {
            return;
        }

        EnemyAlertSignal.Spawn(
            _context.Position,
            receiver,
            _context.LastKnownPlayerPosition,
            enemyConfig.alertSignalSpeed);
    }

    private EnemyAI2D FindNearestAlertReceiver()
    {
        EnemyAI2D[] enemies = FindObjectsByType<EnemyAI2D>(FindObjectsSortMode.None);
        if (enemies == null || enemies.Length == 0)
        {
            return null;
        }

        Vector2 origin = _context.Position;
        float maxSqrDistance = enemyConfig.alertRadius * enemyConfig.alertRadius;
        float nearestSqrDistance = maxSqrDistance;
        EnemyAI2D nearestEnemy = null;

        foreach (EnemyAI2D enemy in enemies)
        {
            if (enemy == null || enemy == this || !enemy.CanReceiveEnemyAlert())
            {
                continue;
            }

            float sqrDistance = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
            if (sqrDistance > nearestSqrDistance)
            {
                continue;
            }

            nearestSqrDistance = sqrDistance;
            nearestEnemy = enemy;
        }

        return nearestEnemy;
    }

    private bool CanReceiveEnemyAlert()
    {
        if (!isActiveAndEnabled)
        {
            return false;
        }

        EnemyState state = CurrentState;
        return state != EnemyState.Chase &&
            state != EnemyState.Attack &&
            state != EnemyState.Hacked;
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
            case EnemyState.Search:
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
        if (string.Equals(rawState, "ReturnToPatrol", StringComparison.OrdinalIgnoreCase))
        {
            return EnemyState.Search;
        }

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
