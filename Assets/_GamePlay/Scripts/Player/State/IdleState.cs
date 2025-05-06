using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    public IdleState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateType stateType) : base(stateMachine, player, stateType)
    {
    }

    public override void Enter()
    {

        player.ResetWallJumpLock();
        player.AnimationController.Play(PlayerStateType.idle);
        base.Enter();
    }


    public override void Update()
    {
        base.Update();
        var input = player.InputHandler;
        var groundCheck = player.GroundCheck;
        float verticalInput = input.MoveInput.y;
        float move = input.MoveInput.x;

        //run
        if (move != 0)
        {
            stateMachine.ChangeState(new RunState(stateMachine, player, PlayerStateType.run));
        }
        //jump
        if (input.JumpPressed && groundCheck.IsGrounded)
        {
            stateMachine.ChangeState(new JumpState(stateMachine, player, PlayerStateType.jump));
        }
        //attack
        if (input.AttackPressed)
        {
            stateMachine.ChangeState(new AttackState(stateMachine, player, PlayerStateType.attack));
        }
        //dash
        if (input.DashPressed && groundCheck.IsGrounded)
        {
            stateMachine.ChangeState(new DashState(stateMachine, player, PlayerStateType.dash));
        }
        //slide
        if (input.SlidePressed && groundCheck.IsGrounded)
        {
            stateMachine.ChangeState(new SlideState(stateMachine, player, PlayerStateType.slide));
        }
        //climb
        if (verticalInput > 0 && groundCheck.IsTouchingLadder)
        {
            stateMachine.ChangeState(new ClimbState(stateMachine, player, PlayerStateType.climb));
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();

    }
    public override void Exit()
    {
        base.Exit();
    }

}
