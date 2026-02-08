public class ChaseState : IEnemyState
{
    private readonly EnemyContext _context;
    private readonly EnemyStateMachine _stateMachine;

    public ChaseState(EnemyContext context, EnemyStateMachine stateMachine)
    {
        _context = context;
        _stateMachine = stateMachine;
    }

    public EnemyState StateType => EnemyState.Chase;

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

        if (_context.DistanceToPlayer <= _context.Config.attackRadius && _context.CanSeePlayer)
        {
            _stateMachine.TransitionTo(EnemyState.Attack);
            return;
        }

        if (_context.TimeSinceSeenPlayer >= _context.Config.loseSightTime)
        {
            _stateMachine.TransitionTo(EnemyState.ReturnToPatrol);
        }
    }

    public void TickFixed(float fixedDeltaTime)
    {
        if (_context.Config == null)
        {
            return;
        }

        _context.MoveTowards(_context.LastKnownPlayerPosition, _context.Config.chaseSpeed, fixedDeltaTime);
    }
}
