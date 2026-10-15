using UnityEngine;

public class WallJumpState : State
{
    private float wallJumpTimer;
    private int jumpDirection;

    // Dùng chung animation jump
    public override PlayerStateType StateType => PlayerStateType.jump;

    public WallJumpState(PlayerStateMachine stateMachine, PlayerController player)
            : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.ConsumeCoyote();

        // Hướng nhảy ngược với hướng mặt (ra khỏi tường)
        jumpDirection = -player.FacingDirection;
        player.LastWallJumpDirection = jumpDirection;

        player.Rigidbody.velocity = new Vector2(jumpDirection * player.stats.wallJumpForceX, player.stats.wallJumpForceY);

        if (jumpDirection != player.FacingDirection)
        {
            player.Flip();
        }

        // Khoá trong WallJumpState một khoảng để giữ quán tính nhảy
        wallJumpTimer = player.stats.wallJumpDuration;

        player.AnimationController.Play(StateType);
    }

    public override void Update()
    {
        base.Update();

        wallJumpTimer -= Time.deltaTime;
        if (wallJumpTimer <= 0)
        {
            ChangeState(States.Fall);
        }
    }
}
