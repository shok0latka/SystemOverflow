public class ReturnToPatrolState : EnemyStateBase
{
    public ReturnToPatrolState(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override EnemyState StateType => EnemyState.ReturnToPatrol;

    public override void Enter()
    {
        Context.ResetReturnTimer();
    }

    public override void Exit() { }

    public override void TickUpdate(float deltaTime)
    {
        Context.ReturnTimer += deltaTime;

        if (Context.CanSeePlayer)
        {
            StateMachine.TransitionTo(EnemyState.Chase);
            return;
        }

        bool reachedLastKnown = Context.IsNear(Context.LastKnownPlayerPosition, 0.2f);
        bool timeoutReached = Context.ReturnTimer >= Config.searchDuration;
        if (reachedLastKnown || timeoutReached)
        {
            StateMachine.TransitionTo(EnemyState.Patrol);
        }
    }

    public override void TickFixed(float fixedDeltaTime)
    {
        Context.MoveTowards(Context.LastKnownPlayerPosition, Config.patrolSpeed, fixedDeltaTime);
    }
}
