public abstract class State
{
    protected readonly PlayerStateMachine stateMachine;
    protected readonly PlayerController player;

    // Dùng làm key animation "{StateType}_{WeaponType}"
    public abstract PlayerStateType StateType { get; }

    protected PlayerStates States => player.States;
    protected bool IsActive => stateMachine.CurrentState == this;

    protected State(PlayerStateMachine stateMachine, PlayerController player)
    {
        this.stateMachine = stateMachine;
        this.player = player;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }

    public virtual void OnWeaponChanged(AnimationController animationController)
    {
        animationController.Play(StateType);
    }

    protected void ChangeState(State newState) => stateMachine.ChangeState(newState);

    // Về state di chuyển mặc định theo việc đang đứng trên đất hay không
    protected void ChangeToDefaultState()
    {
        ChangeState(player.GroundCheck.IsGrounded ? States.Idle : (State)States.Fall);
    }
}
