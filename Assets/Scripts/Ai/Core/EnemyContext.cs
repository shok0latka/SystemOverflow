using UnityEngine;

public class EnemyContext
{
    private const float MinimumInputMagnitude = 0.0001f;

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
            config.suspicionGainPerSecond,
            config.suspicionDecayPerSecond
        );
    }

    public EnemyConfig Config { get; private set; }
    public Rigidbody2D Rigidbody { get; }
    public Transform[] PatrolPoints { get; private set; }
    public Transform Player { get; private set; }
    public PlayerHealth PlayerHealth { get; private set; }
    public LayerMask ObstacleMask { get; set; }
    public EnemyHackController HackController { get; private set; }

    public SuspicionMeter Suspicion { get; }

    public bool CanSeePlayer { get; private set; }
    public float DistanceToPlayer { get; private set; } = float.MaxValue;
    public Vector2 LastKnownPlayerPosition { get; set; }
    public float TimeSinceSeenPlayer { get; set; }
    public Vector2 ViewDirection { get; private set; } = Vector2.right;
    public float CloseVisionRadius => Config.attackRadius;
    public float ViewConeAngleDegrees => Config.visionConeAngleDegrees;
    public float HalfViewConeAngleDegrees => ViewConeAngleDegrees * 0.5f;

    public int PatrolIndex { get; set; }
    public float AttackCooldownTimer { get; set; }
    public float ReturnTimer { get; set; }

    public void SetConfig(EnemyConfig config)
    {
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

    public void SetHackController(EnemyHackController hackController)
    {
        HackController = hackController;
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

        Transform patrolPoint = GetNextValidPatrolPoint();
        if (patrolPoint == null)
        {
            return;
        }

        Vector2 target = patrolPoint.position;
        MoveTowards(target, speed, fixedDeltaTime);

        if (Vector2.Distance(Rigidbody.position, target) < 0.2f)
        {
            PatrolIndex = (PatrolIndex + 1) % PatrolPoints.Length;
        }
    }

    private Transform GetNextValidPatrolPoint()
    {
        for (int offset = 0; offset < PatrolPoints.Length; offset++)
        {
            int candidateIndex = (PatrolIndex + offset) % PatrolPoints.Length;
            Transform candidate = PatrolPoints[candidateIndex];
            if (candidate == null)
            {
                continue;
            }

            PatrolIndex = candidateIndex;
            return candidate;
        }

        return null;
    }

    public void MoveWithRelativeInput(float moveRight, float moveForward, float speed, float fixedDeltaTime)
    {
        if (Rigidbody == null || speed <= 0f)
        {
            return;
        }

        Vector2 input = new Vector2(
            Mathf.Clamp(moveRight, -1f, 1f),
            Mathf.Clamp(moveForward, -1f, 1f));
        if (input.sqrMagnitude < MinimumInputMagnitude)
        {
            return;
        }

        input = Vector2.ClampMagnitude(input, 1f);
        Vector2 forward = ViewDirection.sqrMagnitude < MinimumInputMagnitude
            ? Vector2.right
            : ViewDirection.normalized;
        Vector2 right = new Vector2(forward.y, -forward.x);
        Vector2 moveDirection = right * input.x + forward * input.y;
        if (moveDirection.sqrMagnitude < MinimumInputMagnitude)
        {
            return;
        }

        moveDirection.Normalize();
        Rigidbody.MovePosition(Rigidbody.position + moveDirection * speed * fixedDeltaTime);
    }

    public bool MoveInDirection(Vector2 direction, float distanceStep)
    {
        if (Rigidbody == null || distanceStep <= 0f)
        {
            return false;
        }

        if (direction.sqrMagnitude < MinimumInputMagnitude)
        {
            return false;
        }

        Vector2 moveDirection = direction.normalized;
        Rigidbody.MovePosition(Rigidbody.position + moveDirection * distanceStep);
        return true;
    }

    public void RotateViewDirection(float turnInput, float degreesPerSecond, float deltaTime)
    {
        float clampedTurnInput = Mathf.Clamp(turnInput, -1f, 1f);
        if (Mathf.Abs(clampedTurnInput) < 0.001f || degreesPerSecond <= 0f || deltaTime <= 0f)
        {
            return;
        }

        Vector2 currentDirection = ViewDirection.sqrMagnitude < MinimumInputMagnitude
            ? Vector2.right
            : ViewDirection.normalized;
        float angleDegrees = -clampedTurnInput * degreesPerSecond * deltaTime;
        float radians = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        Vector2 rotatedDirection = new Vector2(
            currentDirection.x * cos - currentDirection.y * sin,
            currentDirection.x * sin + currentDirection.y * cos);
        UpdateViewDirection(rotatedDirection);
    }

    public bool TryInteractWithNearestInteractable()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Position, Config.interactRadius);
        if (colliders == null || colliders.Length == 0)
        {
            return false;
        }

        Interactable nearestInteractable = null;
        float nearestSqrDistance = float.MaxValue;
        Vector2 position = Position;

        foreach (Collider2D collider in colliders)
        {
            if (collider == null)
            {
                continue;
            }

            Interactable interactable = collider.GetComponentInParent<Interactable>();
            if (interactable == null)
            {
                continue;
            }

            Vector2 toInteractable = (Vector2)interactable.transform.position - position;
            float sqrDistance = toInteractable.sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestInteractable = interactable;
            }
        }

        if (nearestInteractable == null)
        {
            return false;
        }

        nearestInteractable.Interact();
        return true;
    }

    public bool TryAttackPlayer()
    {
        if (Player == null || PlayerHealth == null)
        {
            return false;
        }

        if (!CanSeePlayer)
        {
            return false;
        }

        if (AttackCooldownTimer > 0f)
        {
            return false;
        }

        if (DistanceToPlayer > Config.attackRadius)
        {
            return false;
        }

        AttackCooldownTimer = Config.attackCooldown;
        PlayerHealth.TakeDamage(Config.attackDamage);
        return true;
    }

    public void StopMovement()
    {
        if (Rigidbody == null)
        {
            return;
        }

        Rigidbody.velocity = Vector2.zero;
        Rigidbody.angularVelocity = 0f;
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
                Rigidbody.angularVelocity = 0f;
            }
        }
    }

    private bool HasLineOfSightToPlayer()
    {
        if (Player == null || Rigidbody == null)
        {
            return false;
        }

        Vector2 origin = Rigidbody.position;
        Vector2 toPlayer = (Vector2)Player.position - origin;
        float distanceToPlayer = toPlayer.magnitude;
        float visionRadius = Config.visionRadius;
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
