public abstract class EnemyStateBase : IEnemyState
{
    protected EnemyStateBase(EnemyContext context, EnemyStateMachine stateMachine)
    {
        Context = context;
        StateMachine = stateMachine;
    }

    protected EnemyContext Context { get; }
    protected EnemyStateMachine StateMachine { get; }
    protected EnemyConfig Config => Context.Config;

    public abstract EnemyState StateType { get; }

    public abstract void Enter();
    public abstract void Exit();

    public abstract void TickUpdate(float deltaTime);
    public abstract void TickFixed(float fixedDeltaTime);
}
