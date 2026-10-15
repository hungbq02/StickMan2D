// State cha cho các state đứng trên mặt đất (Idle, Run, Land).
// Gom các transition dùng chung để mọi state mặt đất xử lý giống nhau.
public abstract class GroundedState : State
{
    protected const float MoveDeadZone = 0.01f;

    protected GroundedState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.ResetWallJumpLock();
        player.ResetDash();
        player.RefreshCoyote();
    }

    public override void Update()
    {
        base.Update();
        TryGroundedTransitions();
    }

    // Thứ tự = độ ưu tiên (cao -> thấp)
    private void TryGroundedTransitions()
    {
        var input = player.InputHandler;
        var groundCheck = player.GroundCheck;

        //climb
        if (input.MoveInput.y > 0 && groundCheck.IsTouchingLadder)
        {
            ChangeState(States.Climb);
            return;
        }
        //slide
        if (input.SlidePressed && groundCheck.IsGrounded)
        {
            input.ConsumeSlide();
            ChangeState(States.Slide);
            return;
        }
        //dash
        if (input.DashPressed && groundCheck.IsGrounded)
        {
            input.ConsumeDash();
            ChangeState(States.Dash);
            return;
        }
        //attack
        if (input.AttackPressed)
        {
            input.ConsumeAttack();
            ChangeState(States.Attack);
            return;
        }
        //jump
        if (input.JumpPressed && player.CanGroundJump)
        {
            input.ConsumeJump();
            ChangeState(States.Jump);
            return;
        }
        //fall (rời mép / mất mặt đất)
        if (!groundCheck.IsGrounded && player.Rigidbody.velocity.y < 0)
        {
            ChangeState(States.Fall);
        }
    }
}
