using UnityEngine;

public class WallSlideState : State
{

    public WallSlideState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateType stateType)
        : base(stateMachine, player, stateType)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.AnimationController.Play(PlayerStateType.wallSlide);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (player.GroundCheck.IsGrounded)
        {
            stateMachine.ChangeState(new LandState(stateMachine, player, PlayerStateType.land));
            return;
        }

        if (!player.GroundCheck.IsTouchingWall 
            || player.InputHandler.MoveInput.x != player.FacingDirection //hướng đang ấn không trùng với hướng mặt (không trùng hướng wall) => không còn chạm tường
            || player.InputHandler.MoveInput.x == -player.LastWallJumpDirection) // hướng đang ấn là hướng nhảy tường trước đó (không cho wallSlide)
        {
            stateMachine.ChangeState(new FallState(stateMachine, player, PlayerStateType.fall));
            return;
        }

        if (player.InputHandler.JumpPressed)
        {
            stateMachine.ChangeState(new WallJumpState(stateMachine, player, PlayerStateType.wallSlide));
            return;
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
