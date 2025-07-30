using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : State
{
    float moveX;
    public JumpState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateType stateType) : base(stateMachine, player, stateType)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.Rigidbody.velocity = new Vector2(player.Rigidbody.velocity.x, player.stats.jumpForce);
        player.AnimationController.Play(PlayerStateType.jump);

    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();

        //Move
        moveX = player.InputHandler.MoveInput.x;
        player.Rigidbody.velocity = new Vector2(moveX * player.stats.moveSpeed, player.Rigidbody.velocity.y);
    }
    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // Flip sprite
        player.CheckIfShouldFlip(moveX);

        //Wall Slide
        if (player.GroundCheck.IsTouchingWall && moveX == player.FacingDirection && !player.GroundCheck.IsGrounded)
        {
            stateMachine.ChangeState(new WallSlideState(stateMachine, player,PlayerStateType.wallSlide));
            return;
        }

        //AirAttack
        if (player.InputHandler.AttackPressed)
        {
            stateMachine.ChangeState(new AirAttackState(stateMachine, player, PlayerStateType.airAttack));
            return;
        }
        //Fall
        if (player.Rigidbody.velocity.y < 0)
        {
            stateMachine.ChangeState(new FallState(stateMachine, player, PlayerStateType.fall));
        }
        //Dash
        if (player.InputHandler.DashPressed && player.canDash)
        {
            player.canDash = false;
            stateMachine.ChangeState(new DashState(stateMachine, player, PlayerStateType.dash));
            return;
        }
    }
    public override void OnWeaponChanged(AnimationController animationController)
    {
        base.OnWeaponChanged(animationController);
        animationController.Play(PlayerStateType.jump);
    }
}
