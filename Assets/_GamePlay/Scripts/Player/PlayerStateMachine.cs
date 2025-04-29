using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private State currentState;
    public State CurrentState => currentState;

    public void Initialize(State startState)
    {
        currentState = startState;
        currentState.Enter();
    }

    public void ChangeState(State newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

}
