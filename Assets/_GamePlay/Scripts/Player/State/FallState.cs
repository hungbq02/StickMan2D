public class FallState : AirborneState
{
    public override PlayerStateType StateType => PlayerStateType.fall;

    public FallState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.AnimationController.Play(StateType);
    }

    public override void Update()
    {
        base.Update();

        // Landing
        if (player.GroundCheck.IsGrounded)
        {
            ChangeState(States.Land);
            return;
        }

        // Coyote time: vừa rời mép (chưa nhảy) vẫn cho nhảy
        var input = player.InputHandler;
        if (input.JumpPressed && player.CanGroundJump)
        {
            input.ConsumeJump();
            ChangeState(States.Jump);
            return;
        }

        TryAirborneTransitions();
    }
}
