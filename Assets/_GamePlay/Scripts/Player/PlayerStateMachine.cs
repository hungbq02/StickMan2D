using UnityEngine;

// Component giữ state machine của player (logic dùng chung nằm ở StateMachine<TState>)
public class PlayerStateMachine : MonoBehaviour
{
    private readonly StateMachine<State> machine = new StateMachine<State>(nameof(PlayerStateMachine));

    public State CurrentState => machine.CurrentState;

    public void Initialize(State startState) => machine.Initialize(startState);

    public void ChangeState(State newState) => machine.ChangeState(newState);
}
