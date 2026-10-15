// Choáng: không điều khiển được cho tới khi StunEffect hết hiệu lực (dùng animation hit)
public class StunState : State
{
    public override PlayerStateType StateType => PlayerStateType.stun;

    public StunState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.AnimationController.Play(PlayerStateType.hit);
    }

    public override void Update()
    {
        base.Update();

        if (!player.IsStunned)
            ChangeToDefaultState();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.BrakeOnGround();
    }

    public override void OnWeaponChanged(AnimationController animationController)
    {
        animationController.Play(PlayerStateType.hit);
    }
}
