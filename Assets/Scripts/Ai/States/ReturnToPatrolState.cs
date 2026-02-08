using UnityEngine;

public class ReturnToPatrolState : IEnemyState
{
    private readonly EnemyContext _context;
    private readonly EnemyStateMachine _stateMachine;

    public ReturnToPatrolState(EnemyContext context, EnemyStateMachine stateMachine)
    {
        this._context = context;
        this._stateMachine = stateMachine;
    }

    public EnemyState StateType => EnemyState.ReturnToPatrol;

    public void Enter()
    {
        _context.ReturnTimer = 0f;
    }

    public void Exit()
    {
    }

    public void TickUpdate(float deltaTime)
    {
        if (_context.Config == null)
        {
            return;
        }

        _context.ReturnTimer += deltaTime;

        if (_context.CanSeePlayer && _context.Suspicion.IsTriggered(_context.Config.suspicionThreshold))
        {
            _stateMachine.TransitionTo(EnemyState.Chase);
            return;
        }

        bool reachedLastKnown = _context.IsNear(_context.LastKnownPlayerPosition, 0.2f);
        bool timeoutReached = _context.ReturnTimer >= _context.Config.searchDuration;
        if (reachedLastKnown || timeoutReached)
        {
            _stateMachine.TransitionTo(EnemyState.Patrol);
        }
    }

    public void TickFixed(float fixedDeltaTime)
    {
        if (_context.Config == null)
        {
            return;
        }

        _context.MoveTowards(_context.LastKnownPlayerPosition, _context.Config.patrolSpeed, fixedDeltaTime);
    }
}
