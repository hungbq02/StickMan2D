using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    float move;
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
        move = player.InputHandler.MoveInput.x;

        //run
        if (move != 0)
        {
            stateMachine.ChangeState(new RunState(stateMachine, player, PlayerStateType.run));
        }
        //jump
        if (player.InputHandler.JumpPressed && player.GroundCheck.IsGrounded)
        {
            stateMachine.ChangeState(new JumpState(stateMachine, player, PlayerStateType.jump));
        }
        //attack
        if (player.InputHandler.AttackPressed)
        {
            stateMachine.ChangeState(new AttackState(stateMachine, player, PlayerStateType.attack));
        }
        //dash
        if (player.InputHandler.DashPressed && player.GroundCheck.IsGrounded)
        {
            stateMachine.ChangeState(new DashState(stateMachine, player, PlayerStateType.dash));
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
