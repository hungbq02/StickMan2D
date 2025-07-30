using UnityEngine;

public class SlideState : State
{
    private GhostSpriteSpawner ghostSpriteSpawner;

    float slideTime;
    private bool isSlideOver;
    private Vector2 slideDirection;

    Vector2 originalSizeCollider;
    Vector2 originalOffsetCollider;

    float slideHeightMultiplier;

    public SlideState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateType stateType)
        : base(stateMachine, player, stateType)
    {
    }

    public override void Enter()
    {
        base.Enter();
        isSlideOver = false;
        slideTime = 0f;
        // set collider size
        originalSizeCollider = player.Collider.size;
        originalOffsetCollider = player.Collider.offset;
        slideHeightMultiplier = player.stats.slideHeightMultiplier;

        player.Collider.size = new Vector2(originalSizeCollider.x, originalSizeCollider.y * slideHeightMultiplier);
        player.Collider.offset = new Vector2(originalOffsetCollider.x, originalOffsetCollider.y - originalSizeCollider.y * (1 - slideHeightMultiplier) / 2f);

        // get direction player looking at
        slideDirection = new Vector2(player.FacingDirection, 0f).normalized;
        // animation
        player.AnimationController.Play(PlayerStateType.slide);

        // spawn ghost
        ghostSpriteSpawner = player.GetComponentInChildren<GhostSpriteSpawner>();
        if (ghostSpriteSpawner != null)
        {
            // Set the fade duration for the ghost sprite
            float fadeDuration = player.stats.slideDuration;
            ghostSpriteSpawner.BeginSpawn(fadeDuration);
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isSlideOver)
        {
            player.Rigidbody.velocity = slideDirection * player.stats.slideSpeed;
        }
    }

    public override void Update()
    {
        base.Update();
        slideTime += Time.deltaTime;
        if (slideTime >= player.stats.slideDuration || !player.GroundCheck.IsGrounded)
        {
            isSlideOver = true;
            stateMachine.ChangeState(new FallState(stateMachine, player, PlayerStateType.idle));
        }
    }
    public override void Exit()
    {
        base.Exit();
        if (ghostSpriteSpawner != null)
        {
            ghostSpriteSpawner.StopSpawn();
        }
        player.Rigidbody.velocity = new Vector2(0f, player.Rigidbody.velocity.y);
        player.ResetCollider();
    }
    public override void OnWeaponChanged(AnimationController animationController)
    {
        base.OnWeaponChanged(animationController);
        animationController.Play(PlayerStateType.slide);
    }
}

