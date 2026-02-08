public class PatrolState : IEnemyState
{
    private readonly EnemyContext _context;
    private readonly EnemyStateMachine _stateMachine;

    public PatrolState(EnemyContext context, EnemyStateMachine stateMachine)
    {
        _context = context;
        _stateMachine = stateMachine;
    }

    public EnemyState StateType => EnemyState.Patrol;

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

        if (_context.Suspicion.IsTriggered(_context.Config.suspicionThreshold))
        {
            _stateMachine.TransitionTo(EnemyState.Chase);
        }
    }

    public void TickFixed(float fixedDeltaTime)
    {
        if (_context.Config == null)
        {
            return;
        }

        _context.MoveAlongPatrol(_context.Config.patrolSpeed, fixedDeltaTime);
    }
}
