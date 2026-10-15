using UnityEngine;

public class ClimbState : State
{
    private float verticalInput;
    private float originalGravityScale;

    public override PlayerStateType StateType => PlayerStateType.climb;

    public ClimbState(PlayerStateMachine stateMachine, PlayerController player) : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        verticalInput = player.InputHandler.MoveInput.y;

        player.Rigidbody.velocity = Vector2.zero;
        originalGravityScale = player.Rigidbody.gravityScale;
        player.Rigidbody.gravityScale = 0f;

        // Animation climb
        player.AnimationController.Play(StateType);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.Rigidbody.velocity = new Vector2(0f, player.stats.climbSpeed * verticalInput);
    }

    public override void Update()
    {
        base.Update();
        var input = player.InputHandler;
        var groundCheck = player.GroundCheck;

        verticalInput = input.MoveInput.y;
        player.AnimationController.SetSpeedAnimation(Mathf.Abs(verticalInput));

        //jump
        if (input.JumpPressed)
        {
            input.ConsumeJump();
            ChangeState(States.Jump);
            return;
        }
        if (groundCheck.IsGrounded || !groundCheck.IsTouchingLadder)
        {
            ChangeState(States.Idle);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.AnimationController.SetSpeedAnimation(1f);
        player.Rigidbody.gravityScale = originalGravityScale;
    }
}
