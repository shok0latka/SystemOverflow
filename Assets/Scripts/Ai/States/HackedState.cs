public class HackedState : EnemyStateBase
{
    private const float HackTurnDegreesPerSecond = 180f;

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
        Context.StopMovement();
    }

    public override void Exit()
    {
        Context.HackController?.ClearCommand();
        Context.StopMovement();
    }

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
        EnemyHackController hackController = Context.HackController;
        if (hackController == null || !hackController.GetHackStatus().IsActive)
        {
            Context.StopMovement();
            return;
        }

        switch (hackController.ConsumeCommand())
        {
            case EnemyHackController.HackCommand.MoveForward:
                Context.MoveWithRelativeInput(0f, 1f, Config.chaseSpeed, fixedDeltaTime);
                break;
            case EnemyHackController.HackCommand.MoveLeft:
                Context.MoveWithRelativeInput(-1f, 0f, Config.chaseSpeed, fixedDeltaTime);
                break;
            case EnemyHackController.HackCommand.MoveRight:
                Context.MoveWithRelativeInput(1f, 0f, Config.chaseSpeed, fixedDeltaTime);
                break;
            case EnemyHackController.HackCommand.RotateLeft:
                Context.RotateViewDirection(-1f, HackTurnDegreesPerSecond, fixedDeltaTime);
                Context.StopMovement();
                break;
            case EnemyHackController.HackCommand.RotateRight:
                Context.RotateViewDirection(1f, HackTurnDegreesPerSecond, fixedDeltaTime);
                Context.StopMovement();
                break;
            case EnemyHackController.HackCommand.Interact:
                Context.TryInteractWithNearestForwardInteractable();
                Context.StopMovement();
                break;
            default:
                Context.StopMovement();
                break;
        }
    }
}
