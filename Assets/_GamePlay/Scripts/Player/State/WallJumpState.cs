using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class WallJumpState : State
{
    private float wallJumpTimer;
    private int jumpDirection;
    public WallJumpState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateType stateType)
            : base(stateMachine, player, stateType)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // Đặt hướng nhảy là ngược lại hướng mặt
        jumpDirection = -player.FacingDirection;
        player.LastWallJumpDirection = jumpDirection;

        // Gán vận tốc nhảy
        player.Rigidbody.velocity = new Vector2(jumpDirection * player.stats.wallJumpForceX, player.stats.wallJumpForceY);

        if (jumpDirection != player.FacingDirection)
        {
            player.Flip();
        }

        // Gán thời gian tồn tại của WallJumpState
        wallJumpTimer = player.stats.wallJumpDuration;

        player.AnimationController.Play(PlayerStateType.jump);

    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // Sau một khoảng thời gian sẽ chuyển sang trạng thái in-air (rơi hoặc di chuyển)
        wallJumpTimer -= Time.deltaTime;
        if (wallJumpTimer <= 0)
        {
            stateMachine.ChangeState(new FallState(stateMachine, player, PlayerStateType.fall));
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
