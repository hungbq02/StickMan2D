using UnityEngine;

public class DashState : State
{
    private GhostSpriteSpawner ghostSpriteSpawner;

    private float dashTime;

    private bool isDashOver;
    private Vector2 dashDirection;

    public DashState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateType stateType)
        : base(stateMachine, player, stateType)
    {
    }

    public override void Enter()
    {
        base.Enter();

        isDashOver = false;
        dashTime = 0f;

        // get direction player looking at
        dashDirection = new Vector2(player.FacingDirection, 0f).normalized;

        // animation
        player.AnimationController.Play(PlayerStateType.dash);

        // spawn ghost
        ghostSpriteSpawner = player.GetComponentInChildren<GhostSpriteSpawner>();
        if (ghostSpriteSpawner != null)
        {
            // Set the fade duration for the ghost sprite
            float fadeDuration = player.stats.dashDuration;
            ghostSpriteSpawner.BeginSpawn(fadeDuration);
        }

    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!isDashOver)
        {
            player.Rigidbody.velocity = dashDirection * player.stats.dashSpeed;
        }
    }

    public override void Update()
    {
        base.Update();
        player.Collider.enabled = false; // Disable collider during dash
        dashTime += Time.deltaTime;
        if (dashTime >= player.stats.dashDuration)
        {
            isDashOver = true;
            stateMachine.ChangeState(new FallState(stateMachine, player, PlayerStateType.idle));
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.Rigidbody.velocity = new Vector2(0f, player.Rigidbody.velocity.y);
        player.Collider.enabled = true; // Re-enable collider after dash

        // exxit dash => stop spawn ghost
        if (ghostSpriteSpawner != null)
        {
            ghostSpriteSpawner.StopSpawn();
        }
    }
}
