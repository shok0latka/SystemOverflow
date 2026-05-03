using UnityEngine;

public class HackedState : EnemyStateBase
{
    private const float HackTurnDegreesPerSecond = 180f;
    private const float MinimumDirectionMagnitude = 0.0001f;

    private HackCommand _activeCommand = HackCommand.None;
    private float _activeCommandTimeRemaining;
    private float _activeMovementDistanceRemaining;
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
            case HackCommand.MoveLeft:
            case HackCommand.MoveRight:
            case HackCommand.MoveGlobalUp:
            case HackCommand.MoveGlobalDown:
            case HackCommand.MoveGlobalLeft:
            case HackCommand.MoveGlobalRight:
                TickMovementCommand(fixedDeltaTime);
                return;
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
                    Context.TryInteractWithNearestInteractable();
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
        if (!hackController.TryDequeueCommand(out HackQueuedCommand command))
        {
            return false;
        }

        _activeCommand = command.Command;
        _activeMovementDistanceRemaining = command.IsMovement
            ? command.Distance
            : 0f;
        _activeCommandTimeRemaining = UnityEngine.Mathf.Max(
            fixedDeltaTime,
            hackController.CommandStepDuration);
        _activeInteractExecuted = false;
        return true;
    }

    private void TickMovementCommand(float fixedDeltaTime)
    {
        if (!TryGetMovementDirection(_activeCommand, out Vector2 direction) ||
            _activeMovementDistanceRemaining <= 0f)
        {
            Context.StopMovement();
            ResetActiveCommand();
            return;
        }

        float distanceStep = Mathf.Min(
            Config.chaseSpeed * fixedDeltaTime,
            _activeMovementDistanceRemaining);
        if (distanceStep <= 0f)
        {
            Context.StopMovement();
            ResetActiveCommand();
            return;
        }

        if (!Context.MoveInDirection(direction, distanceStep))
        {
            Context.StopMovement();
            ResetActiveCommand();
            return;
        }

        _activeMovementDistanceRemaining -= distanceStep;
        if (_activeMovementDistanceRemaining <= 0f)
        {
            ResetActiveCommand();
        }
    }

    private bool TryGetMovementDirection(HackCommand command, out Vector2 direction)
    {
        Vector2 forward = Context.ViewDirection.sqrMagnitude < MinimumDirectionMagnitude
            ? Vector2.right
            : Context.ViewDirection.normalized;
        Vector2 right = new Vector2(forward.y, -forward.x);

        switch (command)
        {
            case HackCommand.MoveForward:
                direction = forward;
                return true;
            case HackCommand.MoveLeft:
                direction = -right;
                return true;
            case HackCommand.MoveRight:
                direction = right;
                return true;
            case HackCommand.MoveGlobalUp:
                direction = Vector2.up;
                return true;
            case HackCommand.MoveGlobalDown:
                direction = Vector2.down;
                return true;
            case HackCommand.MoveGlobalLeft:
                direction = Vector2.left;
                return true;
            case HackCommand.MoveGlobalRight:
                direction = Vector2.right;
                return true;
            default:
                direction = Vector2.zero;
                return false;
        }
    }

    private void ResetActiveCommand()
    {
        _activeCommand = HackCommand.None;
        _activeCommandTimeRemaining = 0f;
        _activeMovementDistanceRemaining = 0f;
        _activeInteractExecuted = false;
    }
}
