public class RobotCombatState : EnemyStateBase
{
    public RobotCombatState(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override EnemyState StateType => EnemyState.RobotCombat;

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
        if (!AiLevelFeatureFlags.EnemiesCanAttackEnemies)
        {
            EndCombat();
            return;
        }

        if (!Context.HasRobotCombatTarget)
        {
            EndCombat();
        }
    }

    public override void TickFixed(float fixedDeltaTime)
    {
        if (!AiLevelFeatureFlags.EnemiesCanAttackEnemies)
        {
            EndCombat();
            return;
        }

        if (!Context.HasRobotCombatTarget)
        {
            EndCombat();
            return;
        }

        if (Context.IsRobotCombatTargetInAttackRange())
        {
            Context.StopMovement();
            Context.TryAttackRobotCombatTarget();
            return;
        }

        Context.MoveAlongPathToRobotCombatTarget(Config.chaseSpeed, fixedDeltaTime);
    }

    private void EndCombat()
    {
        Context.ClearRobotCombatTarget();
        Context.StopMovement();
        StateMachine.TransitionTo(EnemyState.Patrol);
    }
}
