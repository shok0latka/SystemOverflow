using UnityEngine;

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
        Context.ClearPath();
    }

    public override void Exit()
    {
        Context.ClearPath();
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
            StateMachine.TransitionTo(EnemyState.Search);
        }
    }

    public override void TickFixed(float fixedDeltaTime)
    {
        Vector2 target = Context.Player != null
            ? (Vector2)Context.Player.position
            : Context.LastKnownPlayerPosition;

        Context.MoveAlongPathTo(target, Config.chaseSpeed, fixedDeltaTime);
    }
}
