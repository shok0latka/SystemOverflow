public class ChaseState : EnemyStateBase
{
    public ChaseState(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override EnemyState StateType => EnemyState.Chase;

    public override void Enter()
    {
        Context.ResetReturnTimer();
    }

    public override void Exit()
    {
        
    }

    public override void TickUpdate(float deltaTime)
    {
        if (Context.DistanceToPlayer <= Config.attackRadius && Context.CanSeePlayer)
        {
            StateMachine.TransitionTo(EnemyState.Attack);
            return;
        }

        if (Context.TimeSinceSeenPlayer >= Config.loseSightTime)
        {
            StateMachine.TransitionTo(EnemyState.ReturnToPatrol);
        }
    }

    public override void TickFixed(float fixedDeltaTime)
    {
        Context.MoveTowards(Context.LastKnownPlayerPosition, Config.chaseSpeed, fixedDeltaTime);
    }
}
