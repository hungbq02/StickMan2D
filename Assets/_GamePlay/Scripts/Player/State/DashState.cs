using UnityEngine;

public class DashState : State
{
    private float dashTime;
    private Vector2 dashDirection;

    public override PlayerStateType StateType => PlayerStateType.dash;

    public DashState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();

        dashTime = 0f;
        dashDirection = new Vector2(player.FacingDirection, 0f);
        player.Collider.enabled = false; // Disable collider during dash

        player.AnimationController.Play(StateType);

        if (player.GhostSpawner != null)
            player.GhostSpawner.BeginSpawn(player.stats.dashDuration);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.Rigidbody.velocity = dashDirection * player.stats.dashSpeed;
    }

    public override void Update()
    {
        base.Update();
        dashTime += Time.deltaTime;
        if (dashTime >= player.stats.dashDuration)
        {
            ChangeState(States.Fall);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.SetVelocityX(0f);
        player.Collider.enabled = true; // Re-enable collider after dash

        if (player.GhostSpawner != null)
            player.GhostSpawner.StopSpawn();
    }
}
