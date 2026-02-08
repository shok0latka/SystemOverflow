using System;
using System.Collections.Generic;

public class EnemyStateMachine
{
    private readonly Dictionary<EnemyState, IEnemyState> _states = new();
    private IEnemyState _current;

    public EnemyState CurrentState => _current?.StateType ?? EnemyState.Patrol;
    public event Action<EnemyState, EnemyState> StateChanged;

    public void Register(IEnemyState state)
    {
        if (state == null)
        {
            return;
        }

        _states[state.StateType] = state;
    }

    public void Initialize(EnemyState initialState)
    {
        TransitionTo(initialState);
    }

    public void TickUpdate(float deltaTime)
    {
        _current?.TickUpdate(deltaTime);
    }

    public void TickFixed(float fixedDeltaTime)
    {
        _current?.TickFixed(fixedDeltaTime);
    }

    public void TransitionTo(EnemyState nextState)
    {
        if (!_states.TryGetValue(nextState, out var next))
        {
            return;
        }

        if (_current == next)
        {
            return;
        }

        EnemyState previousState = _current?.StateType ?? next.StateType;
        _current?.Exit();
        _current = next;
        _current.Enter();
        StateChanged?.Invoke(previousState, _current.StateType);
    }
}
