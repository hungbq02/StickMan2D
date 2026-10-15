using UnityEngine;

public class LandState : GroundedState
{
    private const float LandDuration = 0.1f;
    private float landTimer;

    public override PlayerStateType StateType => PlayerStateType.land;

    public LandState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        landTimer = 0f;
        player.AnimationController.Play(StateType);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.BrakeOnGround();
    }

    // Kế thừa GroundedState nên jump/attack/dash/slide có thể huỷ animation tiếp đất
    public override void Update()
    {
        base.Update();
        if (!IsActive) return;

        landTimer += Time.deltaTime;
        if (landTimer >= LandDuration)
        {
            if (Mathf.Abs(player.InputHandler.MoveInput.x) > MoveDeadZone)
                ChangeState(States.Run);
            else
                ChangeState(States.Idle);
        }
    }
}
