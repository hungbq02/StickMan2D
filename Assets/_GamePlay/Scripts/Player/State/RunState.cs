using UnityEngine;

public class RunState : State
{
    float moveInput;
    public RunState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateType stateType) : base(stateMachine, player, stateType)
    {

    }

    public override void Enter()
    {
        base.Enter();
        player.ResetWallJumpLock();

        player.AnimationController.Play(PlayerStateType.run);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.Rigidbody.velocity = new Vector2(moveInput * player.stats.moveSpeed, player.Rigidbody.velocity.y);
    }

    public override void Update()
    {
        base.Update();
        //fall
        if (player.Rigidbody.velocity.y < 0)
            stateMachine.ChangeState(new FallState(stateMachine, player, PlayerStateType.fall));

        moveInput = player.InputHandler.MoveInput.x;


        // Flip sprite
        player.CheckIfShouldFlip(moveInput);

        // idle
        if (Mathf.Abs(moveInput) < 0.01f)
        {
            player.Rigidbody.velocity = new Vector2(0f, player.Rigidbody.velocity.y);
            stateMachine.ChangeState(new IdleState(stateMachine, player, PlayerStateType.idle));
        }

        // jump
        if (player.InputHandler.JumpPressed && player.GroundCheck.IsGrounded)
            stateMachine.ChangeState(new JumpState(stateMachine, player, PlayerStateType.jump));

        // attack
        if (player.InputHandler.AttackPressed)
            stateMachine.ChangeState(new AttackState(stateMachine, player, PlayerStateType.attack));
        // dash
        if (player.InputHandler.DashPressed && player.GroundCheck.IsGrounded)
            stateMachine.ChangeState(new DashState(stateMachine, player, PlayerStateType.dash));
    }

    public override void Exit()
    {
        base.Exit();
    }
}
