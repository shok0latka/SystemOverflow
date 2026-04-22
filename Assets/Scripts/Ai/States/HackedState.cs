using UnityEngine;

public class HackedState : EnemyStateBase
{
    private Vector2 _frozenPosition;

    public HackedState(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override EnemyState StateType => EnemyState.Hacked;

    public override void Enter()
    {
        Context.Suspicion.Reset();
        Context.ResetReturnTimer();
        _frozenPosition = Context.Position;
        Context.Position = _frozenPosition;
        Context.StopMovement();
    }

    public override void Exit() { }

    public override void TickUpdate(float deltaTime)
    {
        Context.HackedTimer -= deltaTime;
        if (Context.HackedTimer <= 0f)
        {
            Context.HackedTimer = 0f;
            StateMachine.TransitionTo(EnemyState.Patrol);
        }
    }

    public override void TickFixed(float fixedDeltaTime)
    {
        Context.Position = _frozenPosition;
        Context.StopMovement();
    }
}
