// State cha cho các state trên không có điều khiển ngang (Jump, Fall).
public abstract class AirborneState : State
{
    protected float moveX;

    protected AirborneState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        moveX = player.InputHandler.MoveInput.x;
    }

    public override void Update()
    {
        base.Update();
        moveX = player.InputHandler.MoveInput.x;
        player.CheckIfShouldFlip(moveX);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.SetVelocityX(player.InputHandler.MoveInput.x * player.stats.moveSpeed);
    }

    // Transition dùng chung trên không. Thứ tự = độ ưu tiên. Trả true nếu đã đổi state.
    protected bool TryAirborneTransitions()
    {
        var input = player.InputHandler;

        //dash
        if (input.DashPressed && player.CanDash)
        {
            input.ConsumeDash();
            player.UseDash();
            ChangeState(States.Dash);
            return true;
        }
        //air attack
        if (input.AttackPressed)
        {
            input.ConsumeAttack();
            ChangeState(States.AirAttack);
            return true;
        }
        //wall slide
        if (CanWallSlide())
        {
            ChangeState(States.WallSlide);
            return true;
        }
        return false;
    }

    private bool CanWallSlide()
    {
        var groundCheck = player.GroundCheck;
        return groundCheck.IsTouchingWall               // chạm tường
            && moveX == player.FacingDirection          // đang ấn hướng trùng với hướng mặt (trùng hướng wall)
            && !groundCheck.IsGrounded                  // không ở dưới đất
            && moveX != -player.LastWallJumpDirection;  // không bám lại bức tường vừa nhảy ra
    }
}
