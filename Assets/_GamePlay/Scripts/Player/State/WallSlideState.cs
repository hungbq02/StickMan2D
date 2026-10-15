public class WallSlideState : State
{
    public override PlayerStateType StateType => PlayerStateType.wallSlide;

    public WallSlideState(PlayerStateMachine stateMachine, PlayerController player)
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
        var input = player.InputHandler;
        var groundCheck = player.GroundCheck;

        if (groundCheck.IsGrounded)
        {
            ChangeState(States.Land);
            return;
        }

        if (!groundCheck.IsTouchingWall)
        {
            ChangeState(States.Fall);
            return;
        }

        // Kiểm tra nhảy trước khi xét buông hướng: bấm hướng ngược + nhảy cùng lúc vẫn ra wall jump
        if (input.JumpPressed)
        {
            input.ConsumeJump();
            ChangeState(States.WallJump);
            return;
        }

        if (input.MoveInput.x != player.FacingDirection         // không còn ấn về phía tường
            || input.MoveInput.x == -player.LastWallJumpDirection) // đang ấn về bức tường vừa nhảy ra
        {
            ChangeState(States.Fall);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (player.Rigidbody.velocity.y < player.stats.wallSlideSpeed)
        {
            player.SetVelocityY(player.stats.wallSlideSpeed);
        }
    }
}
