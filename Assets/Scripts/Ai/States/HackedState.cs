using UnityEngine;

public class HackedState : EnemyStateBase
{
    private HackCommand _activeCommand = HackCommand.None;
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
        Context.ClearRobotCombatTarget();
        Context.ClearPath();
        Context.StopMovement();
    }

    public override void Exit()
    {
        ResetActiveCommand();
        Context.HackController?.ClearCommands();
        Context.ClearPath();
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
            !TryBeginNextCommand(hackController))
        {
            Context.StopMovement();
            return;
        }

        if (HackQueuedCommand.IsMovementCommand(_activeCommand))
        {
            TickMovementCommand(fixedDeltaTime);
            return;
        }

        switch (_activeCommand)
        {
            case HackCommand.Interact:
                if (!_activeInteractExecuted)
                {
                    Context.TryInteractWithNearestInteractable();
                    _activeInteractExecuted = true;
                }

                Context.StopMovement();
                ResetActiveCommand();
                return;
            case HackCommand.Attack:
                if (!_activeInteractExecuted)
                {
                    Context.TryAttackNearestEnemy();
                    _activeInteractExecuted = true;
                }

                Context.StopMovement();
                ResetActiveCommand();
                return;
            case HackCommand.None:
            default:
                Context.StopMovement();
                ResetActiveCommand();
                return;
        }
    }

    private bool TryBeginNextCommand(EnemyHackController hackController)
    {
        if (!hackController.TryDequeueCommand(out HackQueuedCommand command))
        {
            return false;
        }

        _activeCommand = command.Command;
        _activeMovementDistanceRemaining = command.IsMovement
            ? command.Distance
            : 0f;
        _activeInteractExecuted = false;
        return true;
    }

    private void TickMovementCommand(float fixedDeltaTime)
    {
        if (!HackQueuedCommand.TryGetMovementDirection(_activeCommand, out Vector2 direction) ||
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

    private void ResetActiveCommand()
    {
        _activeCommand = HackCommand.None;
        _activeMovementDistanceRemaining = 0f;
        _activeInteractExecuted = false;
    }
}
