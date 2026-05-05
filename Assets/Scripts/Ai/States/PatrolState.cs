public class PatrolState : EnemyStateBase
{
    public PatrolState(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override EnemyState StateType => EnemyState.Patrol;

    public override void Enter()
    {
        Context.ResetReturnTimer();
        Context.ClearPath();
    }

    public override void Exit()
    {
        Context.ClearPath();
    }

    public override void TickUpdate(float deltaTime)
    {
        if (Context.Suspicion.IsTriggered(Config.suspicionThreshold))
        {
            StateMachine.TransitionTo(EnemyState.Chase);
        }
    }

    public override void TickFixed(float fixedDeltaTime)
    {
        Context.MoveAlongPatrol(Config.patrolSpeed, fixedDeltaTime);
    }
}
