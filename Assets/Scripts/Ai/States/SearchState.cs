public class SearchState : EnemyStateBase
{
    private const float LastKnownArrivalDistance = 0.2f;

    private bool _isWaitingAtLastKnownPosition;

    public SearchState(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override EnemyState StateType => EnemyState.Search;

    public override void Enter()
    {
        _isWaitingAtLastKnownPosition = false;
        Context.ResetReturnTimer();
        Context.ClearPath();
    }

    public override void Exit()
    {
        Context.ClearPath();
    }

    public override void TickUpdate(float deltaTime)
    {
        if (Context.CanSeePlayer)
        {
            _isWaitingAtLastKnownPosition = false;
            Context.ResetReturnTimer();
            StateMachine.TransitionTo(EnemyState.Chase);
            return;
        }

        if (!_isWaitingAtLastKnownPosition)
        {
            if (Context.IsNear(Context.LastKnownPlayerPosition, LastKnownArrivalDistance))
            {
                _isWaitingAtLastKnownPosition = true;
                Context.ResetReturnTimer();
                Context.StopMovement();
            }

            return;
        }

        Context.ReturnTimer += deltaTime;
        Context.StopMovement();

        if (Context.ReturnTimer < Config.searchDuration)
        {
            return;
        }

        Context.Suspicion.Reset();
        Context.TimeSinceSeenPlayer = 0f;
        Context.ResetReturnTimer();
        StateMachine.TransitionTo(EnemyState.Patrol);
    }

    public override void TickFixed(float fixedDeltaTime)
    {
        if (_isWaitingAtLastKnownPosition)
        {
            Context.StopMovement();
            return;
        }

        Context.MoveAlongPathTo(Context.LastKnownPlayerPosition, Config.patrolSpeed, fixedDeltaTime);
    }
}
