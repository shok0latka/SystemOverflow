public class AttackState : EnemyStateBase
{
    public AttackState(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override EnemyState StateType => EnemyState.Attack;

    public override void Enter() { }
    public override void Exit() { }

    public override void TickUpdate(float deltaTime)
    {
        if (Config == null)
        {
            return;
        }

        if (Context.DistanceToPlayer > Config.attackRadius)
        {
            if (Context.TimeSinceSeenPlayer >= Config.loseSightTime)
            {
                StateMachine.TransitionTo(EnemyState.ReturnToPatrol);
            }
            else
            {
                StateMachine.TransitionTo(EnemyState.Chase);
            }
            return;
        }

        if (!Context.CanSeePlayer && Context.TimeSinceSeenPlayer >= Config.loseSightTime)
        {
            StateMachine.TransitionTo(EnemyState.ReturnToPatrol);
        }
    }

    public override void TickFixed(float fixedDeltaTime)
    {
        Context.TryAttackPlayer();
    }
}
