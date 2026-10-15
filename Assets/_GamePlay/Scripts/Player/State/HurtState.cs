using UnityEngine;

// Bị trúng đòn: huỷ đòn đang đánh, khoá điều khiển trong stats.hurtDuration (knockback vẫn tác dụng),
// sau đó sang Stun nếu đòn có choáng, không thì về Idle / Fall.
public class HurtState : State
{
    private float timer;

    public override PlayerStateType StateType => PlayerStateType.hit;

    public HurtState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        timer = player.stats.hurtDuration;
        player.AnimationController.Play(StateType);
    }

    // Knockback đẩy lùi rồi hãm dần trên đất (trên không giữ quán tính)
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.BrakeOnGround();
    }

    public override void Update()
    {
        base.Update();

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (player.IsStunned)
            ChangeState(States.Stun);
        else
            ChangeToDefaultState();
    }
}
