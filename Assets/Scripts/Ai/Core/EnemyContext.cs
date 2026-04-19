using UnityEngine;

public class EnemyContext
{
    private const float DefaultViewConeAngleDegrees = 90f;

    private readonly EnemyAI2D _owner;

    public EnemyContext(
        EnemyAI2D owner,
        EnemyConfig config,
        Rigidbody2D rigidbody,
        Transform[] patrolPoints,
        Transform player,
        LayerMask obstacleMask
    )
    {
        _owner = owner;
        Config = config;
        Rigidbody = rigidbody;
        PatrolPoints = patrolPoints;
        Player = player;
        ObstacleMask = obstacleMask;
        Suspicion = new SuspicionMeter(
            config != null ? config.suspicionGainPerSecond : 0.9f,
            config != null ? config.suspicionDecayPerSecond : 0.4f
        );
    }

    public EnemyConfig Config { get; private set; }
    public Rigidbody2D Rigidbody { get; }
    public Transform[] PatrolPoints { get; private set; }
    public Transform Player { get; private set; }
    public PlayerHealth PlayerHealth { get; private set; }
    public LayerMask ObstacleMask { get; set; }

    public SuspicionMeter Suspicion { get; }

    public bool CanSeePlayer { get; private set; }
    public float DistanceToPlayer { get; private set; } = float.MaxValue;
    public Vector2 LastKnownPlayerPosition { get; set; }
    public float TimeSinceSeenPlayer { get; set; }
    public Vector2 ViewDirection { get; private set; } = Vector2.right;
    public float CloseVisionRadius => Config != null ? Mathf.Max(1f, Config.attackRadius) : 1f;
    public float ViewConeAngleDegrees => Config != null
        ? Mathf.Clamp(Config.visionConeAngleDegrees, 1f, 360f)
        : DefaultViewConeAngleDegrees;
    public float HalfViewConeAngleDegrees => ViewConeAngleDegrees * 0.5f;

    public int PatrolIndex { get; set; }
    public float AttackCooldownTimer { get; set; }
    public float ReturnTimer { get; set; }
    public float HackedTimer { get; set; }

    public void SetConfig(EnemyConfig config)
    {
        if (config == null)
        {
            return;
        }

        Config = config;
        Suspicion.Configure(config.suspicionGainPerSecond, config.suspicionDecayPerSecond);
    }

    public void SetPatrolPoints(Transform[] patrolPoints)
    {
        PatrolPoints = patrolPoints;
    }

    public void SetPlayer(Transform player)
    {
        Player = player;
        PlayerHealth = player == null ? null : player.GetComponent<PlayerHealth>();
    }

    public void EnsurePlayerReference()
    {
        if (Player != null)
        {
            if (PlayerHealth == null)
            {
                PlayerHealth = Player.GetComponent<PlayerHealth>();
            }
            return;
        }

        PlayerMovement playerMovement = Object.FindObjectOfType<PlayerMovement>();
        if (playerMovement == null)
        {
            return;
        }

        SetPlayer(playerMovement.transform);
    }

    public void TickPerception(float deltaTime, bool updateSuspicion)
    {
        EnsurePlayerReference();

        if (Rigidbody == null)
        {
            CanSeePlayer = false;
            DistanceToPlayer = float.MaxValue;
            TimeSinceSeenPlayer += deltaTime;
            if (updateSuspicion)
            {
                Suspicion.Tick(false, deltaTime);
            }
            else
            {
                Suspicion.Reset();
            }
            return;
        }

        CanSeePlayer = HasLineOfSightToPlayer();
        DistanceToPlayer = Player == null
            ? float.MaxValue
            : Vector2.Distance(Rigidbody.position, Player.position);

        if (CanSeePlayer && Player != null)
        {
            LastKnownPlayerPosition = Player.position;
            TimeSinceSeenPlayer = 0f;
        }
        else
        {
            TimeSinceSeenPlayer += deltaTime;
        }

        if (updateSuspicion)
        {
            Suspicion.Tick(CanSeePlayer, deltaTime);
        }
        else
        {
            Suspicion.Reset();
        }
    }

    public void TickCooldowns(float deltaTime)
    {
        AttackCooldownTimer = Mathf.Max(0f, AttackCooldownTimer - deltaTime);
    }

    public void UpdateViewDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        ViewDirection = direction.normalized;
    }

    public void UpdateViewDirectionTowards(Vector2 target)
    {
        UpdateViewDirection(target - Position);
    }

    public void ResetReturnTimer()
    {
        ReturnTimer = 0f;
    }

    public void MoveTowards(Vector2 target, float speed, float fixedDeltaTime)
    {
        if (Rigidbody == null || speed <= 0f)
        {
            return;
        }

        Vector2 direction = target - Rigidbody.position;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        direction.Normalize();
        Rigidbody.MovePosition(Rigidbody.position + direction * speed * fixedDeltaTime);
    }

    public void MoveAlongPatrol(float speed, float fixedDeltaTime)
    {
        if (PatrolPoints == null || PatrolPoints.Length == 0 || Rigidbody == null)
        {
            return;
        }

        if (PatrolIndex < 0 || PatrolIndex >= PatrolPoints.Length)
        {
            PatrolIndex = 0;
        }

        Vector2 target = PatrolPoints[PatrolIndex].position;
        MoveTowards(target, speed, fixedDeltaTime);

        if (Vector2.Distance(Rigidbody.position, target) < 0.2f)
        {
            PatrolIndex = (PatrolIndex + 1) % PatrolPoints.Length;
        }
    }

    public bool TryAttackPlayer()
    {
        if (Config == null || Player == null || PlayerHealth == null)
        {
            return false;
        }

        if (AttackCooldownTimer > 0f)
        {
            return false;
        }

        if (DistanceToPlayer > Mathf.Max(0.1f, Config.attackRadius))
        {
            return false;
        }

        AttackCooldownTimer = Mathf.Max(0.05f, Config.attackCooldown);
        PlayerHealth.TakeDamage(Mathf.Max(1, Config.attackDamage));
        return true;
    }

    public void StartHack(float baseDuration)
    {
        float safeDuration = baseDuration > 0f
            ? baseDuration
            : Config != null
                ? Config.baseHackDuration
                : 6f;

        float resistance = Config != null ? Mathf.Clamp01(Config.hackResistance) : 0f;
        HackedTimer = Mathf.Max(0.2f, safeDuration * (1f - resistance));
        AttackCooldownTimer = 0f;
        ReturnTimer = 0f;
        TimeSinceSeenPlayer = 0f;
        Suspicion.Reset();
    }

    public bool IsNear(Vector2 target, float threshold)
    {
        if (Rigidbody == null)
        {
            return false;
        }

        return Vector2.Distance(Rigidbody.position, target) <= threshold;
    }

    public Vector2 Position
    {
        get => Rigidbody != null ? Rigidbody.position : (Vector2)_owner.transform.position;
        set
        {
            _owner.transform.position = value;

            if (Rigidbody != null)
            {
                Rigidbody.position = value;
                Rigidbody.velocity = Vector2.zero;
            }
        }
    }

    private bool HasLineOfSightToPlayer()
    {
        if (Player == null || Rigidbody == null || Config == null)
        {
            return false;
        }

        Vector2 origin = Rigidbody.position;
        Vector2 toPlayer = (Vector2)Player.position - origin;
        float distanceToPlayer = toPlayer.magnitude;
        float visionRadius = Mathf.Max(0.1f, Config.visionRadius);
        float closeVisionRadius = CloseVisionRadius;
        if (distanceToPlayer > Mathf.Max(visionRadius, closeVisionRadius))
        {
            return false;
        }

        if (distanceToPlayer < 0.0001f)
        {
            return true;
        }

        if (!HasClearPathToPlayer(origin, toPlayer, distanceToPlayer))
        {
            return false;
        }

        if (distanceToPlayer <= closeVisionRadius)
        {
            return true;
        }

        if (distanceToPlayer > visionRadius)
        {
            return false;
        }

        Vector2 directionToPlayer = toPlayer / distanceToPlayer;
        float minimumDot = Mathf.Cos(HalfViewConeAngleDegrees * Mathf.Deg2Rad);
        return Vector2.Dot(ViewDirection, directionToPlayer) >= minimumDot;
    }

    private bool HasClearPathToPlayer(Vector2 origin, Vector2 toPlayer, float distanceToPlayer)
    {
        if (ObstacleMask.value == 0)
        {
            return true;
        }

        RaycastHit2D hit = Physics2D.Raycast(origin, toPlayer / distanceToPlayer, distanceToPlayer, ObstacleMask);
        return hit.collider == null;
    }
}
