// State dùng được với StateMachine<TState> (player, enemy...)
public interface IState
{
    void Enter();
    void Update();
    void FixedUpdate();
    void Exit();
}
