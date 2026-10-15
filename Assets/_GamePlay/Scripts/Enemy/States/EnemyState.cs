// Base cho state của EnemyController (chạy bằng StateMachine<EnemyState>)
public abstract class EnemyState : IState
{
    protected readonly EnemyController enemy;

    protected EnemyDataSO Data => enemy.Data;

    protected EnemyState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }

    protected void ChangeState(EnemyState state) => enemy.ChangeState(state);
}
