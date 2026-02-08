public class HackedState : IEnemyState
{
    private readonly EnemyContext _context;
    private readonly EnemyStateMachine _stateMachine;

    public HackedState(EnemyContext context, EnemyStateMachine stateMachine)
    {
        _context = context;
        _stateMachine = stateMachine;
    }

    public EnemyState StateType => EnemyState.Hacked;

    public void Enter()
    {
        _context.Suspicion.Reset();
        _context.ReturnTimer = 0f;
    }

    public void Exit()
    {
    }

    public void TickUpdate(float deltaTime)
    {
        _context.HackedTimer -= deltaTime;
        if (_context.HackedTimer <= 0f)
        {
            _context.HackedTimer = 0f;
            _stateMachine.TransitionTo(EnemyState.Patrol);
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
