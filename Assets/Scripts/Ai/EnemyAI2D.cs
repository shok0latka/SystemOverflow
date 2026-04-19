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

    private EnemyContext context;
    private EnemyStatusIndicator statusIndicator;
    private EnemyVisionOutline visionOutline;
    private EnemyStateMachine stateMachine;

    private EnemyConfig boundConfig;
    private Transform[] boundPatrolPoints;
    private Transform boundPlayer;
    private int boundObstacleMask;
    private bool bindingsDirty = true;

    public string SaveId => persistentId;

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

        ConfigureStatusIndicator(allowCreate: true);
        ConfigureVisionOutline(allowCreate: true);
        InitializeRuntime();
        ApplyContextBindings(force: true);
        SyncDebugRuntime();
    }

    private void FixedUpdate()
    {
        if (context == null || stateMachine == null)
        {
            return;
        }

        ApplyContextBindings(force: false);
        UpdateViewDirectionForCurrentState(stateMachine.CurrentState);
        context.TickCooldowns(Time.fixedDeltaTime);
        context.TickPerception(Time.fixedDeltaTime, stateMachine.CurrentState != EnemyState.Hacked);

        stateMachine.TickUpdate(Time.fixedDeltaTime);
        stateMachine.TickFixed(Time.fixedDeltaTime);
        SyncDebugRuntime();
    }

    private void LateUpdate()
    {
        statusIndicator?.RefreshPresentation();
        RefreshVisionOutline(allowCreate: true);
    }

    private void OnValidate()
    {
        EnsurePersistentId(ensureUniqueInScene: !Application.isPlaying);

        if (!Application.isPlaying)
        {
            return;
        }

        bindingsDirty = true;
        ConfigureStatusIndicator(allowCreate: false);
        ConfigureVisionOutline(allowCreate: false);
        ApplyContextBindings(force: false);
        RefreshVisionOutline(allowCreate: false);
    }

    private void OnDestroy()
    {
        if (stateMachine != null)
        {
            stateMachine.StateChanged -= HandleStateChanged;
        }
    }

    public bool TryHack(float durationSeconds)
    {
        if (context == null || stateMachine == null)
        {
            return false;
        }

        if (stateMachine.CurrentState == EnemyState.Hacked && context.HackedTimer > 0f)
        {
            return false;
        }

        context.StartHack(durationSeconds);
        stateMachine.TransitionTo(EnemyState.Hacked);
        return true;
    }

    public EnemyRuntimeSaveData CaptureRuntimeState()
    {
        EnemyState state = stateMachine?.CurrentState ?? EnemyState.Patrol;
        Vector2 position = context?.Position ?? (Vector2)transform.position;
        Vector2 lastKnown = context?.LastKnownPlayerPosition ?? position;

        return new EnemyRuntimeSaveData
        {
            saveId = SaveId,
            posX = position.x,
            posY = position.y,
            state = state.ToString(),
            patrolIndex = context?.PatrolIndex ?? 0,
            suspicion = context?.Suspicion.Value ?? 0f,
            timeSinceSeen = context?.TimeSinceSeenPlayer ?? 0f,
            attackTimer = context?.AttackCooldownTimer ?? 0f,
            searchTimer = context?.ReturnTimer ?? 0f,
            hackedTimer = context?.HackedTimer ?? 0f,
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

        if (context == null || stateMachine == null)
        {
            if (!ValidateDependencies())
            {
                return;
            }

            InitializeRuntime();
            ApplyContextBindings(force: true);
        }

        context.Position = new Vector2(data.posX, data.posY);
        context.PatrolIndex = Mathf.Max(0, data.patrolIndex);
        context.Suspicion.Set(data.suspicion);
        context.TimeSinceSeenPlayer = Mathf.Max(0f, data.timeSinceSeen);
        context.AttackCooldownTimer = Mathf.Max(0f, data.attackTimer);
        context.ReturnTimer = Mathf.Max(0f, data.searchTimer);
        context.HackedTimer = Mathf.Max(0f, data.hackedTimer);
        context.LastKnownPlayerPosition = new Vector2(data.lastKnownX, data.lastKnownY);

        EnemyState restoredState = ParseState(data.state);
        if (restoredState == EnemyState.Hacked && context.HackedTimer <= 0f)
        {
            context.HackedTimer = 0.2f;
        }

        stateMachine.TransitionTo(restoredState);
        UpdateViewDirectionForCurrentState(stateMachine.CurrentState);
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
        if (stateMachine != null)
        {
            stateMachine.StateChanged -= HandleStateChanged;
        }

        context = new EnemyContext(this, enemyConfig, rb, patrolPoints, player, obstacleMask);
        context.EnsurePlayerReference();

        if (player == null && context.Player != null)
        {
            player = context.Player;
        }

        if (context.Player != null)
        {
            context.LastKnownPlayerPosition = context.Player.position;
        }

        stateMachine = new EnemyStateMachine();
        stateMachine.StateChanged += HandleStateChanged;
        stateMachine.Register(new PatrolState(context, stateMachine));
        stateMachine.Register(new ChaseState(context, stateMachine));
        stateMachine.Register(new AttackState(context, stateMachine));
        stateMachine.Register(new HackedState(context, stateMachine));
        stateMachine.Register(new ReturnToPatrolState(context, stateMachine));
        stateMachine.Initialize(EnemyState.Patrol);
        UpdateViewDirectionForCurrentState(stateMachine.CurrentState);
    }

    private void ApplyContextBindings(bool force)
    {
        if (context == null)
        {
            return;
        }

        bool configChanged = boundConfig != enemyConfig;
        bool patrolChanged = !ReferenceEquals(boundPatrolPoints, patrolPoints);
        bool playerChanged = boundPlayer != player;
        bool obstacleChanged = boundObstacleMask != obstacleMask.value;
        bool shouldApply = force || bindingsDirty || configChanged || patrolChanged || playerChanged || obstacleChanged;

        if (!shouldApply)
        {
            return;
        }

        if (enemyConfig != null)
        {
            context.SetConfig(enemyConfig);
        }

        context.SetPatrolPoints(patrolPoints);
        context.ObstacleMask = obstacleMask;

        if (playerChanged || force || bindingsDirty)
        {
            context.SetPlayer(player);
        }

        context.EnsurePlayerReference();
        if (player == null && context.Player != null)
        {
            player = context.Player;
        }

        boundConfig = enemyConfig;
        boundPatrolPoints = patrolPoints;
        boundPlayer = player;
        boundObstacleMask = obstacleMask.value;
        bindingsDirty = false;
    }

    private void HandleStateChanged(EnemyState _, EnemyState toState)
    {
        currentState = toState;
        statusIndicator?.ApplyState(currentState);
    }

    private void SyncDebugRuntime()
    {
        if (context != null)
        {
            suspicion = context.Suspicion.Value;
        }
    }

    private void ConfigureStatusIndicator(bool allowCreate)
    {
        statusIndicator = EnsureStatusIndicatorComponent(allowCreate);
        if (statusIndicator == null)
        {
            return;
        }

        statusIndicator.Configure(statusText, statusOffset, allowCreate);
        statusIndicator.ApplyState(currentState, allowCreate);
    }

    private void ConfigureVisionOutline(bool allowCreate)
    {
        visionOutline = EnsureVisionOutlineComponent(allowCreate);
    }

    private EnemyStatusIndicator EnsureStatusIndicatorComponent(bool allowCreate)
    {
        if (statusIndicator != null)
        {
            return statusIndicator;
        }

        statusIndicator = GetComponent<EnemyStatusIndicator>();
        if (statusIndicator == null)
        {
            if (!allowCreate)
            {
                return null;
            }

            statusIndicator = gameObject.AddComponent<EnemyStatusIndicator>();
        }

        return statusIndicator;
    }

    private EnemyVisionOutline EnsureVisionOutlineComponent(bool allowCreate)
    {
        if (visionOutline != null)
        {
            return visionOutline;
        }

        visionOutline = GetComponent<EnemyVisionOutline>();
        if (visionOutline == null)
        {
            if (!allowCreate)
            {
                return null;
            }

            visionOutline = gameObject.AddComponent<EnemyVisionOutline>();
        }

        return visionOutline;
    }

    private void RefreshVisionOutline(bool allowCreate)
    {
        visionOutline = EnsureVisionOutlineComponent(allowCreate);
        if (visionOutline == null)
        {
            return;
        }

        Vector3 origin = context != null ? (Vector3)context.Position : transform.position;
        Vector2 viewDirection = context != null ? context.ViewDirection : Vector2.right;
        float radius = enemyConfig != null ? Mathf.Max(0f, enemyConfig.visionRadius) : 0f;
        float closeVisionRadius = context != null
            ? context.CloseVisionRadius
            : enemyConfig != null
                ? Mathf.Max(1f, enemyConfig.attackRadius)
                : 1f;
        float coneAngleDegrees = context != null
            ? context.ViewConeAngleDegrees
            : enemyConfig != null
                ? Mathf.Clamp(enemyConfig.visionConeAngleDegrees, 1f, 360f)
                : 90f;
        bool shouldShow = context != null && currentState != EnemyState.Hacked;

        visionOutline.RefreshOutline(
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
        if (context == null)
        {
            return;
        }

        switch (state)
        {
            case EnemyState.Patrol:
            case EnemyState.Hacked:
                if (TryGetPatrolFacingTarget(out Vector2 patrolTarget))
                {
                    context.UpdateViewDirectionTowards(patrolTarget);
                }
                break;
            case EnemyState.Chase:
                context.UpdateViewDirectionTowards(context.LastKnownPlayerPosition);
                break;
            case EnemyState.Attack:
                if (context.Player != null)
                {
                    context.UpdateViewDirectionTowards(context.Player.position);
                }
                break;
            case EnemyState.ReturnToPatrol:
                context.UpdateViewDirectionTowards(context.LastKnownPlayerPosition);
                break;
        }
    }

    private bool TryGetPatrolFacingTarget(out Vector2 patrolTarget)
    {
        patrolTarget = default;

        Transform[] points = context?.PatrolPoints;
        if (points == null || points.Length == 0)
        {
            return false;
        }

        int patrolIndex = context.PatrolIndex;
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
        if (context.IsNear(patrolTarget, 0.2f))
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
