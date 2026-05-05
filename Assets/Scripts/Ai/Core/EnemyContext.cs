using System.Collections.Generic;
using UnityEngine;

public class EnemyContext
{
    private const float MinimumInputMagnitude = 0.0001f;
    private const int MovementCastCapacity = 8;
    private const int LineOfSightCastCapacity = 8;
    private const float MovementSkinWidth = 0.02f;

    private readonly EnemyAI2D _owner;
    private readonly RaycastHit2D[] _movementHits = new RaycastHit2D[MovementCastCapacity];
    private readonly RaycastHit2D[] _lineOfSightHits = new RaycastHit2D[LineOfSightCastCapacity];
    private readonly EnemyPathNavigator _pathNavigator;

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
        _pathNavigator = new EnemyPathNavigator(owner, rigidbody);
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
    public EnemyHealth Health { get; private set; }

    public SuspicionMeter Suspicion { get; }

    public bool CanSeePlayer { get; private set; }
    public float DistanceToPlayer { get; private set; } = float.MaxValue;
    public Vector2 LastKnownPlayerPosition { get; set; }
    public bool HasActiveSearchTarget { get; private set; }
    public Vector2 ActiveSearchTargetPosition { get; private set; }
    public float TimeSinceSeenPlayer { get; set; }
    public Vector2 ViewDirection { get; private set; } = Vector2.right;
    public float CloseVisionRadius => Config.attackRadius;
    public float ViewConeAngleDegrees => Config.visionConeAngleDegrees;
    public float HalfViewConeAngleDegrees => ViewConeAngleDegrees * 0.5f;

    public int PatrolIndex { get; set; }
    public float AttackCooldownTimer { get; set; }
    public float ReturnTimer { get; set; }
    public EnemyAI2D RobotCombatTarget { get; private set; }
    public bool HasRobotCombatTarget => IsValidEnemyTarget(RobotCombatTarget);
    public Vector2 RobotCombatTargetPosition => HasRobotCombatTarget
        ? RobotCombatTarget.Position
        : Position;

    public void SetConfig(EnemyConfig config)
    {
        Config = config;
        Suspicion.Configure(config.suspicionGainPerSecond, config.suspicionDecayPerSecond);
        ClearPath();
    }

    public void SetPatrolPoints(Transform[] patrolPoints)
    {
        PatrolPoints = patrolPoints;
        ClearPath();
    }

    public void SetPlayer(Transform player)
    {
        Player = player;
        PlayerHealth = player == null ? null : player.GetComponent<PlayerHealth>();
        ClearPath();
    }

    public void SetHackController(EnemyHackController hackController)
    {
        HackController = hackController;
    }

    public void SetHealth(EnemyHealth health)
    {
        Health = health;
    }

    public void SetRobotCombatTarget(EnemyAI2D target)
    {
        RobotCombatTarget = IsValidEnemyTarget(target) ? target : null;
        ClearPath();
    }

    public void ClearRobotCombatTarget()
    {
        RobotCombatTarget = null;
        ClearPath();
    }

    public void SetActiveSearchTarget(Vector2 target)
    {
        ActiveSearchTargetPosition = target;
        HasActiveSearchTarget = true;
    }

    public void ClearActiveSearchTarget()
    {
        ActiveSearchTargetPosition = default;
        HasActiveSearchTarget = false;
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

    public void ClearPath()
    {
        _pathNavigator.Clear();
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

        MoveWithCollision(direction, speed * fixedDeltaTime);
    }

    public bool MoveAlongPathTo(Vector2 target, float speed, float fixedDeltaTime)
    {
        if (Rigidbody == null || speed <= 0f)
        {
            return false;
        }

        if (!_pathNavigator.TryGetMoveDirection(
                Rigidbody.position,
                target,
                Config,
                Player,
                fixedDeltaTime,
                out Vector2 direction))
        {
            return false;
        }

        bool moved = MoveWithCollisionAvoidance(direction, speed * fixedDeltaTime);
        if (!moved)
        {
            _pathNavigator.RequestRefresh();
        }

        return moved;
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
        MoveAlongPathTo(target, speed, fixedDeltaTime);

        if (Vector2.Distance(Rigidbody.position, target) < 0.2f)
        {
            PatrolIndex = (PatrolIndex + 1) % PatrolPoints.Length;
            ClearPath();
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

        return MoveWithCollision(direction, distanceStep);
    }

    private bool MoveWithCollision(Vector2 direction, float distance)
    {
        if (Rigidbody == null || distance <= 0f || direction.sqrMagnitude < MinimumInputMagnitude)
        {
            return false;
        }

        Vector2 moveDirection = direction.normalized;
        float allowedDistance = GetCollisionLimitedDistance(moveDirection, distance);
        if (allowedDistance <= 0f)
        {
            return false;
        }

        Rigidbody.MovePosition(Rigidbody.position + moveDirection * allowedDistance);
        return true;
    }

    private bool MoveWithCollisionAvoidance(Vector2 direction, float distance)
    {
        if (Rigidbody == null || distance <= 0f || direction.sqrMagnitude < MinimumInputMagnitude)
        {
            return false;
        }

        Vector2 moveDirection = direction.normalized;
        bool blockedByEnemy =
            TryGetMovementBlocker(moveDirection, distance, out Collider2D blocker) &&
            IsEnemyCollider(blocker);

        if (blockedByEnemy && TryStepAroundEnemy(moveDirection, distance))
        {
            return true;
        }

        return MoveWithCollision(moveDirection, distance);
    }

    private bool TryStepAroundEnemy(Vector2 moveDirection, float distance)
    {
        Vector2 side = new(-moveDirection.y, moveDirection.x);
        if (_owner != null && (_owner.GetInstanceID() & 1) == 0)
        {
            side = -side;
        }

        Vector2 forwardSide = (moveDirection + side).normalized;
        Vector2 oppositeForwardSide = (moveDirection - side).normalized;

        return MoveWithCollision(forwardSide, distance)
            || MoveWithCollision(side, distance)
            || MoveWithCollision(oppositeForwardSide, distance)
            || MoveWithCollision(-side, distance);
    }

    private bool TryGetMovementBlocker(Vector2 direction, float distance, out Collider2D blocker)
    {
        blocker = null;

        if (Rigidbody == null || direction.sqrMagnitude < MinimumInputMagnitude)
        {
            return false;
        }

        int hitCount = Rigidbody.Cast(direction, _movementHits, distance + MovementSkinWidth);
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = _movementHits[i];
            if (!IsBlockingMovementHit(hit) || hit.distance >= nearestDistance)
            {
                continue;
            }

            nearestDistance = hit.distance;
            blocker = hit.collider;
        }

        return blocker != null;
    }

    private bool IsEnemyCollider(Collider2D collider)
    {
        EnemyAI2D enemy = collider != null ? collider.GetComponentInParent<EnemyAI2D>() : null;
        return enemy != null && enemy != _owner;
    }

    private float GetCollisionLimitedDistance(Vector2 direction, float distance)
    {
        int hitCount = Rigidbody.Cast(direction, _movementHits, distance + MovementSkinWidth);
        float allowedDistance = distance;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = _movementHits[i];
            if (!IsBlockingMovementHit(hit))
            {
                continue;
            }

            float hitDistance = Mathf.Max(0f, hit.distance - MovementSkinWidth);
            allowedDistance = Mathf.Min(allowedDistance, hitDistance);
        }

        return allowedDistance;
    }

    private static bool IsBlockingMovementHit(RaycastHit2D hit)
    {
        return hit.collider != null && !hit.collider.isTrigger;
    }

    public void RotateWorld(float degreesPerSecond, float deltaTime)
    {
        if (Rigidbody == null ||
            Mathf.Abs(degreesPerSecond) < 0.001f ||
            deltaTime <= 0f)
        {
            return;
        }

        Rigidbody.MoveRotation(Rigidbody.rotation + degreesPerSecond * deltaTime);
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

    public bool MoveAlongPathToRobotCombatTarget(float speed, float fixedDeltaTime)
    {
        if (!HasRobotCombatTarget)
        {
            return false;
        }

        return MoveAlongPathTo(RobotCombatTarget.Position, speed, fixedDeltaTime);
    }

    public bool IsRobotCombatTargetInAttackRange()
    {
        return HasRobotCombatTarget &&
            Vector2.Distance(Position, RobotCombatTarget.Position) <= Config.attackRadius;
    }

    public bool TryAttackRobotCombatTarget()
    {
        return TryAttackEnemy(RobotCombatTarget);
    }

    public bool TryAttackNearestEnemy()
    {
        return TryAttackEnemy(FindNearestEnemyTarget());
    }

    private bool TryAttackEnemy(EnemyAI2D target)
    {
        if (!AiLevelFeatureFlags.EnemiesCanAttackEnemies)
        {
            return false;
        }

        if (!IsValidEnemyTarget(target))
        {
            return false;
        }

        if (AttackCooldownTimer > 0f)
        {
            return false;
        }

        if (Vector2.Distance(Position, target.Position) > Config.attackRadius)
        {
            return false;
        }

        AttackCooldownTimer = Config.attackCooldown;
        return target.Health.TakeDamage(Config.attackDamage, _owner);
    }

    private EnemyAI2D FindNearestEnemyTarget()
    {
        EnemyAI2D[] enemies = Object.FindObjectsByType<EnemyAI2D>(FindObjectsSortMode.None);
        if (enemies == null || enemies.Length == 0)
        {
            return null;
        }

        Vector2 position = Position;
        float maxSqrDistance = Config.attackRadius * Config.attackRadius;
        float nearestSqrDistance = maxSqrDistance;
        EnemyAI2D nearestEnemy = null;

        foreach (EnemyAI2D enemy in enemies)
        {
            if (!IsValidEnemyTarget(enemy))
            {
                continue;
            }

            float sqrDistance = (enemy.Position - position).sqrMagnitude;
            if (sqrDistance > nearestSqrDistance)
            {
                continue;
            }

            nearestSqrDistance = sqrDistance;
            nearestEnemy = enemy;
        }

        return nearestEnemy;
    }

    private bool IsValidEnemyTarget(EnemyAI2D target)
    {
        return target != null &&
            target != _owner &&
            target.isActiveAndEnabled &&
            target.Health != null &&
            target.Health.IsAlive;
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
            ClearPath();

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

        Vector2 directionToPlayer = toPlayer / distanceToPlayer;
        int hitCount = Physics2D.RaycastNonAlloc(
            origin,
            directionToPlayer,
            _lineOfSightHits,
            distanceToPlayer,
            ObstacleMask);

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = _lineOfSightHits[i];
            if (IsBlockingLineOfSightHit(hit))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsBlockingLineOfSightHit(RaycastHit2D hit)
    {
        Collider2D collider = hit.collider;
        if (collider == null || collider.isTrigger)
        {
            return false;
        }

        if (collider.GetComponentInParent<EnemyAI2D>() != null)
        {
            return false;
        }

        Transform hitTransform = collider.transform;
        if (Player != null && (hitTransform == Player || hitTransform.IsChildOf(Player)))
        {
            return false;
        }

        return true;
    }
}

internal sealed class EnemyPathNavigator
{
    private const int ObstacleProbeCapacity = 32;
    private const float MinimumTargetMoveThreshold = 0.05f;
    private const float MinimumWaypointArrivalDistance = 0.05f;

    private static readonly Vector2Int[] Neighbors =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private readonly EnemyAI2D _owner;
    private readonly Rigidbody2D _rigidbody;
    private readonly Collider2D[] _obstacleProbeHits = new Collider2D[ObstacleProbeCapacity];
    private readonly List<Vector2> _path = new();
    private readonly List<Vector2Int> _open = new();
    private readonly HashSet<Vector2Int> _closed = new();
    private readonly Dictionary<Vector2Int, Vector2Int> _cameFrom = new();
    private readonly Dictionary<Vector2Int, int> _costByCell = new();

    private Vector2 _target;
    private int _pathIndex;
    private float _refreshTimer;

    public EnemyPathNavigator(EnemyAI2D owner, Rigidbody2D rigidbody)
    {
        _owner = owner;
        _rigidbody = rigidbody;
    }

    public void Clear()
    {
        _path.Clear();
        _pathIndex = 0;
        _refreshTimer = 0f;
        _target = default;
    }

    public void RequestRefresh()
    {
        _refreshTimer = 0f;
    }

    public bool TryGetMoveDirection(
        Vector2 start,
        Vector2 target,
        EnemyConfig config,
        Transform player,
        float deltaTime,
        out Vector2 direction)
    {
        direction = Vector2.zero;

        if (config == null)
        {
            return false;
        }

        _refreshTimer -= Mathf.Max(0f, deltaTime);
        if (ShouldRebuildPath(target, config))
        {
            RebuildPath(start, target, config, player);
        }

        if (_path.Count == 0)
        {
            return false;
        }

        AdvancePastReachedWaypoints(start, config.pathCellSize);
        Vector2 waypoint = GetCurrentWaypoint(target);
        Vector2 toWaypoint = waypoint - start;
        if (toWaypoint.sqrMagnitude < MinimumTargetMoveThreshold * MinimumTargetMoveThreshold)
        {
            return false;
        }

        direction = toWaypoint.normalized;
        return true;
    }

    private bool ShouldRebuildPath(Vector2 target, EnemyConfig config)
    {
        if (_path.Count == 0 || _pathIndex >= _path.Count)
        {
            return true;
        }

        if (_refreshTimer <= 0f)
        {
            return true;
        }

        float targetMoveThreshold = Mathf.Max(MinimumTargetMoveThreshold, config.pathCellSize * 0.5f);
        return (_target - target).sqrMagnitude >= targetMoveThreshold * targetMoveThreshold;
    }

    private void RebuildPath(
        Vector2 start,
        Vector2 target,
        EnemyConfig config,
        Transform player)
    {
        _path.Clear();
        _pathIndex = 0;
        _target = target;
        _refreshTimer = config.pathRefreshInterval;

        float cellSize = config.pathCellSize;
        Vector2Int startCell = WorldToCell(start, cellSize);
        Vector2Int goalCell = ResolveGoalCell(
            startCell,
            WorldToCell(target, cellSize),
            cellSize,
            player);

        if (startCell == goalCell)
        {
            _path.Add(target);
            return;
        }

        if (!TryFindPath(startCell, goalCell, config, player))
        {
            return;
        }

        _path[_path.Count - 1] = target;
    }

    private bool TryFindPath(
        Vector2Int startCell,
        Vector2Int goalCell,
        EnemyConfig config,
        Transform player)
    {
        _open.Clear();
        _closed.Clear();
        _cameFrom.Clear();
        _costByCell.Clear();

        _open.Add(startCell);
        _costByCell[startCell] = 0;

        int searchedNodes = 0;
        while (_open.Count > 0 && searchedNodes < config.pathMaxSearchNodes)
        {
            searchedNodes++;
            Vector2Int current = RemoveBestOpenCell(goalCell);
            if (current == goalCell)
            {
                BuildPath(startCell, goalCell, config.pathCellSize);
                return _path.Count > 0;
            }

            _closed.Add(current);

            foreach (Vector2Int offset in Neighbors)
            {
                Vector2Int neighbor = current + offset;
                if (_closed.Contains(neighbor) ||
                    IsCellBlocked(neighbor, config.pathCellSize, player))
                {
                    continue;
                }

                int newCost = _costByCell[current] + 1;
                if (_costByCell.TryGetValue(neighbor, out int oldCost) && oldCost <= newCost)
                {
                    continue;
                }

                _costByCell[neighbor] = newCost;
                _cameFrom[neighbor] = current;
                if (!_open.Contains(neighbor))
                {
                    _open.Add(neighbor);
                }
            }
        }

        _path.Clear();
        return false;
    }

    private Vector2Int RemoveBestOpenCell(Vector2Int goalCell)
    {
        int bestIndex = 0;
        int bestScore = GetEstimatedCost(_open[0], goalCell);

        for (int i = 1; i < _open.Count; i++)
        {
            int score = GetEstimatedCost(_open[i], goalCell);
            if (score >= bestScore)
            {
                continue;
            }

            bestIndex = i;
            bestScore = score;
        }

        Vector2Int bestCell = _open[bestIndex];
        _open.RemoveAt(bestIndex);
        return bestCell;
    }

    private int GetEstimatedCost(Vector2Int cell, Vector2Int goalCell)
    {
        return _costByCell[cell] + GetManhattanDistance(cell, goalCell);
    }

    private void BuildPath(Vector2Int startCell, Vector2Int goalCell, float cellSize)
    {
        _path.Clear();

        Vector2Int current = goalCell;
        while (current != startCell)
        {
            _path.Add(CellToWorld(current, cellSize));
            if (!_cameFrom.TryGetValue(current, out Vector2Int previous))
            {
                _path.Clear();
                return;
            }

            current = previous;
        }

        _path.Reverse();
    }

    private Vector2Int ResolveGoalCell(
        Vector2Int startCell,
        Vector2Int goalCell,
        float cellSize,
        Transform player)
    {
        if (startCell == goalCell || !IsCellBlocked(goalCell, cellSize, player))
        {
            return goalCell;
        }

        const int maxGoalSearchRadius = 4;
        Vector2Int bestCell = goalCell;
        int bestDistance = int.MaxValue;

        for (int radius = 1; radius <= maxGoalSearchRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector2Int candidate = goalCell + new Vector2Int(x, y);
                    if (GetManhattanDistance(candidate, goalCell) > radius ||
                        IsCellBlocked(candidate, cellSize, player))
                    {
                        continue;
                    }

                    int distanceToStart = GetManhattanDistance(candidate, startCell);
                    if (distanceToStart >= bestDistance)
                    {
                        continue;
                    }

                    bestCell = candidate;
                    bestDistance = distanceToStart;
                }
            }

            if (bestDistance < int.MaxValue)
            {
                return bestCell;
            }
        }

        return bestCell;
    }

    private bool IsCellBlocked(
        Vector2Int cell,
        float cellSize,
        Transform player)
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            CellToWorld(cell, cellSize),
            GetProbeRadius(cellSize),
            _obstacleProbeHits);

        for (int i = 0; i < hitCount; i++)
        {
            if (IsNavigationObstacle(_obstacleProbeHits[i], player))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsNavigationObstacle(Collider2D collider, Transform player)
    {
        if (collider == null || collider.isTrigger)
        {
            return false;
        }

        if (_rigidbody != null && collider.attachedRigidbody == _rigidbody)
        {
            return false;
        }

        Transform hitTransform = collider.transform;
        if (_owner != null && hitTransform.IsChildOf(_owner.transform))
        {
            return false;
        }

        if (player != null && (hitTransform == player || hitTransform.IsChildOf(player)))
        {
            return false;
        }

        return collider.GetComponentInParent<EnemyAI2D>() == null;
    }

    private void AdvancePastReachedWaypoints(Vector2 start, float cellSize)
    {
        float arrivalDistance = Mathf.Max(MinimumWaypointArrivalDistance, cellSize * 0.25f);
        float arrivalSqrDistance = arrivalDistance * arrivalDistance;

        while (_pathIndex < _path.Count &&
            (_path[_pathIndex] - start).sqrMagnitude <= arrivalSqrDistance)
        {
            _pathIndex++;
        }
    }

    private Vector2 GetCurrentWaypoint(Vector2 target)
    {
        return _pathIndex < _path.Count
            ? _path[_pathIndex]
            : target;
    }

    private static Vector2Int WorldToCell(Vector2 position, float cellSize)
    {
        return new Vector2Int(
            Mathf.RoundToInt(position.x / cellSize),
            Mathf.RoundToInt(position.y / cellSize));
    }

    private static Vector2 CellToWorld(Vector2Int cell, float cellSize)
    {
        return new Vector2(cell.x * cellSize, cell.y * cellSize);
    }

    private float GetProbeRadius(float cellSize)
    {
        float radius = Mathf.Max(0.05f, cellSize * 0.45f);
        Collider2D bodyCollider = _rigidbody != null ? _rigidbody.GetComponent<Collider2D>() : null;
        if (bodyCollider == null)
        {
            return radius;
        }

        Vector3 extents = bodyCollider.bounds.extents;
        return Mathf.Max(radius, Mathf.Max(extents.x, extents.y));
    }

    private static int GetManhattanDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }
}
