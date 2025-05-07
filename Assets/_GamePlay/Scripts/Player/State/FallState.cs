using UnityEngine;

public class FallState : State
{
    private float moveX;

    public FallState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateType stateType)
        : base(stateMachine, player, stateType)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.AnimationController.Play(PlayerStateType.fall);
    }

    public override void Update()
    {
        base.Update();

        moveX = player.InputHandler.MoveInput.x;

        // Landing
        if (player.GroundCheck.IsGrounded)
        {
            stateMachine.ChangeState(new LandState(stateMachine, player, PlayerStateType.land));
        }

        // Dash
        if (player.InputHandler.DashPressed && player.canDash)
        {
            player.canDash = false;
            stateMachine.ChangeState(new DashState(stateMachine, player, PlayerStateType.dash));
            return;
        }

        // AirAttack
        if (player.InputHandler.AttackPressed)
        {
            stateMachine.ChangeState(new AirAttackState(stateMachine, player, PlayerStateType.airAttack));
            return;
        }

        // WallSlide
        if (player.GroundCheck.IsTouchingWall   //Cham tường
            && moveX == player.FacingDirection  //đang ấn hướng trùng với hướng mặt ( trùng hướng wall)
            && !player.GroundCheck.IsGrounded    //Không ở dưới đất
            && moveX != -player.LastWallJumpDirection) //hướng đang ấn không phải là hướng nhảy tường trước đó
        {
            stateMachine.ChangeState(new WallSlideState(stateMachine, player, PlayerStateType.wallSlide));
            return;
        }

        //Flip
        player.CheckIfShouldFlip(moveX);
    }


    public override void FixedUpdate()
    {
        base.FixedUpdate();

        float horizontal = player.InputHandler.MoveInput.x;
        Vector2 velocity = player.Rigidbody.velocity;
        velocity.x = horizontal * player.stats.moveSpeed;
        player.Rigidbody.velocity = velocity;
    }    
    public override void Exit()
    {
        base.Exit();
    }

}
