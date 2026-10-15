using UnityEngine;

// Hết máu: phát animation death (1 lần, giữ frame cuối), khoá điều khiển, sau stats.respawnDelay thì hồi sinh.
// Damageable tự bỏ qua mọi đòn khi Health hết => không cần chặn thêm.
public class DeadState : State
{
    private float timer;

    public override PlayerStateType StateType => PlayerStateType.death;

    public DeadState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        timer = player.stats.respawnDelay;
        player.MeleeHandler.Cancel();
        player.GunHandler.Cancel();
        player.AnimationController.Play(StateType);
    }

    public override void Update()
    {
        base.Update();

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        player.Respawn();
        ChangeToDefaultState();
    }

    // Rơi tự do nếu chết trên không, trên đất thì hãm lại (không trượt theo knockback)
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.BrakeOnGround();
    }
}
