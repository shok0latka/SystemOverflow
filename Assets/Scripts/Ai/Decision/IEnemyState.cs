public interface IEnemyState
{
    EnemyState StateType { get; }
    void Enter();
    void Exit();
    void TickUpdate(float deltaTime);
    void TickFixed(float fixedDeltaTime);
}
