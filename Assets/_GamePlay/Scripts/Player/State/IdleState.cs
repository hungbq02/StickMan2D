using UnityEngine;

public class IdleState : GroundedState
{
    public override PlayerStateType StateType => PlayerStateType.idle;

    public IdleState(PlayerStateMachine stateMachine, PlayerController player) : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.AnimationController.Play(StateType);
    }

    public override void Update()
    {
        base.Update();
        if (!IsActive) return;

        //run
        if (Mathf.Abs(player.InputHandler.MoveInput.x) > MoveDeadZone)
        {
            ChangeState(States.Run);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.BrakeOnGround(); // vận tốc dư (knockback) không làm nhân vật trượt mãi
    }

    public override void OnWeaponChanged(AnimationController animationController)
    {
        animationController.SetSpeedAnimation(1f);
        base.OnWeaponChanged(animationController);
    }
}
