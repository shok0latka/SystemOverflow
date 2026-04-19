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
    }

    public override void Exit() { }

    public override void TickUpdate(float deltaTime)
    {
        if (Config == null)
        {
            return;
        }

        if (Context.Suspicion.IsTriggered(Config.suspicionThreshold))
        {
            StateMachine.TransitionTo(EnemyState.Chase);
        }
    }

    public override void TickFixed(float fixedDeltaTime)
    {
        if (Config == null)
        {
            return;
        }

        Context.MoveAlongPatrol(Config.patrolSpeed, fixedDeltaTime);
    }
}
