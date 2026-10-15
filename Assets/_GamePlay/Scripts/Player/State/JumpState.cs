public class JumpState : AirborneState
{
    public override PlayerStateType StateType => PlayerStateType.jump;

    public JumpState(PlayerStateMachine stateMachine, PlayerController player) : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.ConsumeCoyote();
        player.SetVelocityY(player.stats.jumpForce);
        player.AnimationController.Play(StateType);
    }

    public override void Update()
    {
        base.Update();

        if (TryAirborneTransitions()) return;

        //Fall
        if (player.Rigidbody.velocity.y < 0)
        {
            ChangeState(States.Fall);
        }
    }
}
