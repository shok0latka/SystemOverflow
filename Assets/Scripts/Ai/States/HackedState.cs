public class HackedState : EnemyStateBase
{
    private const float TurnSpeedDegreesPerSecond = 180f;

    public HackedState(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override EnemyState StateType => EnemyState.Hacked;

    public override void Enter()
    {
        Context.Suspicion.Reset();
        Context.ResetReturnTimer();
        Context.HackController?.BeginHackControl();
    }

    public override void Exit()
    {
        Context.HackController?.EndHackControl();
    }

    public override void TickUpdate(float deltaTime)
    {
        if (Context.HackController != null && Context.HackController.ConsumeInteractRequest())
        {
            Context.TryInteractWithNearestForwardInteractable();
        }

        Context.HackedTimer -= deltaTime;
        if (Context.HackedTimer <= 0f)
        {
            Context.HackedTimer = 0f;
            StateMachine.TransitionTo(EnemyState.Patrol);
        }
    }

    public override void TickFixed(float fixedDeltaTime)
    {
        EnemyHackController hackController = Context.HackController;
        if (hackController == null)
        {
            return;
        }

        HackedEnemyDriveIntent driveIntent = hackController.CurrentDriveIntent;
        Context.RotateViewDirection(driveIntent.Turn, TurnSpeedDegreesPerSecond, fixedDeltaTime);

        if (Config != null)
        {
            Context.MoveWithRelativeInput(
                driveIntent.MoveRight,
                driveIntent.MoveForward,
                Config.patrolSpeed,
                fixedDeltaTime);
        }
    }
}
