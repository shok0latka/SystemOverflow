public class HackedState : EnemyStateBase
{
    private const float HackTurnDegreesPerSecond = 180f;

    private HackCommand _activeCommand = HackCommand.None;
    private float _activeCommandTimeRemaining;
    private bool _activeInteractExecuted;

    public HackedState(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override EnemyState StateType => EnemyState.Hacked;

    public override void Enter()
    {
        ResetActiveCommand();
        Context.Suspicion.Reset();
        Context.ResetReturnTimer();
        Context.AttackCooldownTimer = 0f;
        Context.TimeSinceSeenPlayer = 0f;
        Context.StopMovement();
    }

    public override void Exit()
    {
        ResetActiveCommand();
        Context.HackController?.ClearCommands();
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
            ResetActiveCommand();
            Context.StopMovement();
            return;
        }

        if (_activeCommand == HackCommand.None &&
            !TryBeginNextCommand(hackController, fixedDeltaTime))
        {
            Context.StopMovement();
            return;
        }

        switch (_activeCommand)
        {
            case HackCommand.MoveForward:
                Context.MoveWithRelativeInput(0f, 1f, Config.chaseSpeed, fixedDeltaTime);
                break;
            case HackCommand.MoveLeft:
                Context.MoveWithRelativeInput(-1f, 0f, Config.chaseSpeed, fixedDeltaTime);
                break;
            case HackCommand.MoveRight:
                Context.MoveWithRelativeInput(1f, 0f, Config.chaseSpeed, fixedDeltaTime);
                break;
            case HackCommand.RotateLeft:
                Context.RotateViewDirection(-1f, HackTurnDegreesPerSecond, fixedDeltaTime);
                Context.StopMovement();
                break;
            case HackCommand.RotateRight:
                Context.RotateViewDirection(1f, HackTurnDegreesPerSecond, fixedDeltaTime);
                Context.StopMovement();
                break;
            case HackCommand.Interact:
                if (!_activeInteractExecuted)
                {
                    Context.TryInteractWithNearestForwardInteractable();
                    _activeInteractExecuted = true;
                }

                Context.StopMovement();
                ResetActiveCommand();
                return;
            case HackCommand.None:
                Context.StopMovement();
                break;
            default:
                Context.StopMovement();
                break;
        }

        _activeCommandTimeRemaining -= fixedDeltaTime;
        if (_activeCommandTimeRemaining <= 0f)
        {
            ResetActiveCommand();
        }
    }

    private bool TryBeginNextCommand(EnemyHackController hackController, float fixedDeltaTime)
    {
        if (!hackController.TryDequeueCommand(out HackCommand command))
        {
            return false;
        }

        _activeCommand = command;
        _activeCommandTimeRemaining = UnityEngine.Mathf.Max(
            fixedDeltaTime,
            hackController.CommandStepDuration);
        _activeInteractExecuted = false;
        return true;
    }

    private void ResetActiveCommand()
    {
        _activeCommand = HackCommand.None;
        _activeCommandTimeRemaining = 0f;
        _activeInteractExecuted = false;
    }
}
