using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class EnemyRuntimeSaveData
{
    public string saveId;
    public float posX;
    public float posY;
    public string state;
    public int patrolIndex;
    public float suspicion;
    public float timeSinceSeen;
    public float attackTimer;
    public float searchTimer;
    public float hackedTimer;
    public float lastKnownX;
    public float lastKnownY;
}

public class EnemyAI2D : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string persistentId;

    [Header("Config")]
    [FormerlySerializedAs("archetype")]
    public EnemyConfig enemyConfig;

    [Header("Refs")]
    public Rigidbody2D rb;
    public Transform[] patrolPoints;
    public Transform player;

    [Header("Perception")]
    public LayerMask obstacleMask;

    [Header("Status Indicator")]
    public TextMesh statusText;
    public Vector3 statusOffset = new Vector3(0f, 1.4f, 0f);

    [Header("Debug Runtime")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrol;
    [SerializeField, Range(0f, 1f)] private float suspicion;

    private EnemyContext context;
    private EnemyStateMachine stateMachine;
    private Camera mainCamera;

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

        EnsureStatusIndicator();
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
        context.TickCooldowns(Time.fixedDeltaTime);
        context.TickPerception(Time.fixedDeltaTime, stateMachine.CurrentState != EnemyState.Hacked);

        stateMachine.TickUpdate(Time.fixedDeltaTime);
        stateMachine.TickFixed(Time.fixedDeltaTime);
        SyncDebugRuntime();
    }

    private void LateUpdate()
    {
        if (statusText == null)
        {
            return;
        }

        statusText.transform.localPosition = statusOffset;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            statusText.transform.rotation = mainCamera.transform.rotation;
        }
    }

    private void OnValidate()
    {
        EnsurePersistentId(ensureUniqueInScene: !Application.isPlaying);

        if (!Application.isPlaying)
        {
            return;
        }

        bindingsDirty = true;
        ApplyContextBindings(force: false);
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
        UpdateStatusVisual();
    }

    private void SyncDebugRuntime()
    {
        if (context != null)
        {
            suspicion = context.Suspicion.Value;
        }
    }

    private void EnsureStatusIndicator()
    {
        if (statusText != null)
        {
            return;
        }

        Transform existing = transform.Find("StateIndicator");
        if (existing != null)
        {
            statusText = existing.GetComponent<TextMesh>();
            if (statusText != null)
            {
                return;
            }
        }

        GameObject indicator = new GameObject("StateIndicator");
        indicator.transform.SetParent(transform, false);
        indicator.transform.localPosition = statusOffset;

        statusText = indicator.AddComponent<TextMesh>();
        statusText.text = "P";
        statusText.fontSize = 72;
        statusText.characterSize = 0.08f;
        statusText.anchor = TextAnchor.MiddleCenter;
        statusText.alignment = TextAlignment.Center;

        MeshRenderer meshRenderer = statusText.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 2000;
        }
    }

    private void UpdateStatusVisual()
    {
        if (statusText == null)
        {
            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                statusText.text = "P";
                statusText.color = new Color(0.55f, 0.95f, 0.55f);
                break;
            case EnemyState.Chase:
                statusText.text = "C";
                statusText.color = new Color(1f, 0.9f, 0.3f);
                break;
            case EnemyState.Attack:
                statusText.text = "A";
                statusText.color = new Color(1f, 0.35f, 0.35f);
                break;
            case EnemyState.Hacked:
                statusText.text = "H";
                statusText.color = new Color(0.8f, 0.55f, 1f);
                break;
            case EnemyState.ReturnToPatrol:
                statusText.text = "R";
                statusText.color = new Color(0.45f, 0.95f, 1f);
                break;
        }
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
