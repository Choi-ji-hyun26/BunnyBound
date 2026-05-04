using UnityEngine;

public class EnemyStateMachine
{
    private IEnemyState currentState;

    public IEnemyState CurrentState => currentState;

    public void Initialize(IEnemyState startState)
    {
        currentState = startState;
        currentState.Enter();
    }

    public void ChangeState(IEnemyState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }
}
