public class AttackState : IEnemyState
{
    private readonly EnemyContext _context;
    private readonly EnemyStateMachine _stateMachine;

    public AttackState(EnemyContext context, EnemyStateMachine stateMachine)
    {
        _context = context;
        _stateMachine = stateMachine;
    }

    public EnemyState StateType => EnemyState.Attack;

    public void Enter()
    {
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

        if (_context.DistanceToPlayer > _context.Config.attackRadius)
        {
            if (_context.TimeSinceSeenPlayer >= _context.Config.loseSightTime)
            {
                _stateMachine.TransitionTo(EnemyState.ReturnToPatrol);
            }
            else
            {
                _stateMachine.TransitionTo(EnemyState.Chase);
            }
            return;
        }

        if (!_context.CanSeePlayer && _context.TimeSinceSeenPlayer >= _context.Config.loseSightTime)
        {
            _stateMachine.TransitionTo(EnemyState.ReturnToPatrol);
        }
    }

    public void TickFixed(float fixedDeltaTime)
    {
        _context.TryAttackPlayer();
    }
}
