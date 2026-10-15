using UnityEngine;

public class RunState : GroundedState
{
    private float moveInput;

    public override PlayerStateType StateType => PlayerStateType.run;

    public RunState(PlayerStateMachine stateMachine, PlayerController player) : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        moveInput = player.InputHandler.MoveInput.x;
        player.AnimationController.Play(StateType);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.SetVelocityX(moveInput * player.stats.moveSpeed);
    }

    public override void Update()
    {
        moveInput = player.InputHandler.MoveInput.x;

        // Flip sprite
        player.CheckIfShouldFlip(moveInput);

        base.Update();
        if (!IsActive) return;

        // idle
        if (Mathf.Abs(moveInput) < MoveDeadZone)
        {
            player.SetVelocityX(0f);
            ChangeState(States.Idle);
        }
    }
}
