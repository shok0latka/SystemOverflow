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
        Context.AttackCooldownTimer = 0f;
        Context.TimeSinceSeenPlayer = 0f;
        _frozenPosition = Context.Position;
        Context.Position = _frozenPosition;
        Context.StopMovement();
    }

    public override void Exit() { }

    public override void TickUpdate(float deltaTime)
    {
        HackStatusSnapshot hackStatus = Context.HackController != null
            ? Context.HackController.GetHackStatus()
            : HackStatusSnapshot.Unavailable;
        if (!hackStatus.IsActive)
        {
            StateMachine.TransitionTo(EnemyState.Patrol);
        }
    }

    public override void TickFixed(float fixedDeltaTime)
    {
        Context.Position = _frozenPosition;
        Context.StopMovement();
    }
}
