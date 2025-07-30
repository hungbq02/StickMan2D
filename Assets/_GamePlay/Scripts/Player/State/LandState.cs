using UnityEngine;

public class LandState : State
{
    private float landTimer;
    private float landDuration = 0.005f; 

    public LandState(PlayerStateMachine stateMachine, PlayerController player,PlayerStateType stateType)
        : base(stateMachine, player, stateType)
    {
    }

    public override void Enter()
    {
        player.ResetWallJumpLock();
        player.canDash = true;
        base.Enter();
        player.AnimationController.Play(PlayerStateType.jump);

        landTimer = 0f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        landTimer += Time.deltaTime;

        if (landTimer >= landDuration)
        {
            if (Mathf.Abs(player.InputHandler.MoveInput.x) > 0.01f)
            {
                stateMachine.ChangeState(new RunState(stateMachine, player, PlayerStateType.run));
            }
            else
            {
                stateMachine.ChangeState(new IdleState(stateMachine, player, PlayerStateType.idle));
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    public override void OnWeaponChanged(AnimationController animationController)
    {
        base.OnWeaponChanged(animationController);
        animationController.Play(PlayerStateType.land);
    }
}
