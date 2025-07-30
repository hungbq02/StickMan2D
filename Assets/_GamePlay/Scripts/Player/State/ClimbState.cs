using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbState : State
{
    float verticalInput;
    float climbSpeed;
    bool isClimbing;
    public ClimbState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateType stateType) : base(stateMachine, player, stateType )
    {
    }

    public override void Enter()
    {
        player.Rigidbody.velocity = Vector2.zero;
        player.Rigidbody.gravityScale = 0f;

        // Animation climb
        player.AnimationController.Play(PlayerStateType.climb);
        climbSpeed = player.stats.climbSpeed;
    }


    public override void FixedUpdate()
    {
        verticalInput = player.InputHandler.MoveInput.y;
        Vector2 climbVelocity = new Vector2(0, climbSpeed * verticalInput);
        player.Rigidbody.velocity = climbVelocity;
    }    
    
    public override void Update()
    {
        float speed = verticalInput >= 0 ? verticalInput : -verticalInput;
        player.AnimationController.SetSpeedAnimation(speed);

        isClimbing = player.GroundCheck.IsTouchingLadder;

        //jump
        if (player.InputHandler.JumpPressed)
        {
            player.SetVelocityY(player.stats.jumpForce);
            stateMachine.ChangeState(new JumpState(stateMachine, player, PlayerStateType.jump));
            return;
        }
        if (player.GroundCheck.IsGrounded || !isClimbing)
        {
            stateMachine.ChangeState(new IdleState(stateMachine, player, PlayerStateType.idle));
            return;
        }


    }
    public override void Exit()
    {
        base.Exit();
        player.AnimationController.SetSpeedAnimation(1f);
        player.Rigidbody.gravityScale = 3f;
    }

    public override void OnWeaponChanged(AnimationController animationController)
    {
        base.OnWeaponChanged(animationController);
        animationController.Play(PlayerStateType.climb);
    }
}
