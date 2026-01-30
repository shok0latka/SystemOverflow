using UnityEngine;

public class EnemyAI2D : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack, Search }

    [Header("Refs")]
    public Rigidbody2D rb;
    public Transform[] patrolPoints;
    public Transform player;

    [Header("Perception")]
    public float detectRadius = 6f;
    public float attackRadius = 1.2f;
    public LayerMask obstacleMask;
    public float loseSightTime = 1.2f;

    [Header("Movement")]
    public float patrolSpeed = 2.0f;
    public float chaseSpeed = 3.2f;

    [Header("Attack")]
    public float attackCooldown = 0.8f;

    private State _state = State.Patrol;
    private int _patrolIndex;
    private Vector2 _lastKnownPlayerPos;
    private float _timeSinceSeen;
    private float _attackTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!player || !rb)
            return;

        bool canSeePlayer = CanSeePlayer();
        float distToPlayer = Vector2.Distance(rb.position, player.position);

        // Обновляем "память"
        if (canSeePlayer)
        {
            _lastKnownPlayerPos = player.position;
            _timeSinceSeen = 0f;
        }
        else
        {
            _timeSinceSeen += Time.deltaTime;
        }

        // Переходы (KISS)
        switch (_state)
        {
            case State.Patrol:
                if (distToPlayer <= detectRadius && canSeePlayer) _state = State.Chase;
                break;

            case State.Chase:
                if (distToPlayer <= attackRadius) _state = State.Attack;
                else if (_timeSinceSeen >= loseSightTime) _state = State.Search;
                break;

            case State.Attack:
                if (distToPlayer > attackRadius) _state = State.Chase;
                break;

            case State.Search:
                // дошёл до lastKnown -> обратно в патруль
                if (Vector2.Distance(rb.position, _lastKnownPlayerPos) < 0.2f)
                    _state = State.Patrol;
                // если снова увидел — chase
                if (distToPlayer <= detectRadius && canSeePlayer) _state = State.Chase;
                break;
        }

        _attackTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (player == null || rb == null) return;

        Vector2 target;
        float speed;

        switch (_state)
        {
            case State.Patrol:
                if (patrolPoints == null || patrolPoints.Length == 0) return;
                target = patrolPoints[_patrolIndex].position;
                speed = patrolSpeed;
                MoveTowards(target, speed);

                if (Vector2.Distance(rb.position, target) < 0.2f)
                    _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
                break;

            case State.Chase:
                target = _lastKnownPlayerPos;
                speed = chaseSpeed;
                MoveTowards(target, speed);
                break;

            case State.Search:
                target = _lastKnownPlayerPos;
                speed = patrolSpeed;
                MoveTowards(target, speed);
                break;

            case State.Attack:
                TryAttack();
                break;
        }
    }

    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 dir = (target - rb.position);
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        Vector2 next = rb.position + dir * (speed * Time.fixedDeltaTime);
        rb.MovePosition(next);
    }

    private bool CanSeePlayer()
    {
        Vector2 origin = rb.position;
        Vector2 toPlayer = (Vector2)player.position - origin;

        if (toPlayer.magnitude > detectRadius) return false;

        RaycastHit2D hit = Physics2D.Raycast(origin, toPlayer.normalized, toPlayer.magnitude, obstacleMask);
        return hit.collider == null;
    }

    private void TryAttack()
    {
        if (_attackTimer > 0f)
            return;

        _attackTimer = attackCooldown;
        Debug.Log("Enemy attack!");
    }
}
