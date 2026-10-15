using UnityEngine;

public class SlideState : State
{
    private float slideTime;
    private Vector2 slideDirection;

    public override PlayerStateType StateType => PlayerStateType.slide;

    public SlideState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        slideTime = 0f;
        player.ShrinkColliderHeight(player.stats.slideHeightMultiplier);

        slideDirection = new Vector2(player.FacingDirection, 0f);
        player.AnimationController.Play(StateType);

        if (player.GhostSpawner != null)
            player.GhostSpawner.BeginSpawn(player.stats.slideDuration);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.Rigidbody.velocity = slideDirection * player.stats.slideSpeed;
    }

    public override void Update()
    {
        base.Update();
        slideTime += Time.deltaTime;
        if (slideTime >= player.stats.slideDuration || !player.GroundCheck.IsGrounded)
        {
            ChangeState(States.Fall);
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (player.GhostSpawner != null)
            player.GhostSpawner.StopSpawn();

        player.SetVelocityX(0f);
        player.ResetCollider();
    }
}
