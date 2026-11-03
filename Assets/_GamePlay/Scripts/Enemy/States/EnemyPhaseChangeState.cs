using UnityEngine;

// Qua ngưỡng máu: đứng yên phát animation chuyển phase (enrage / phase / roar), bất tử nếu phase yêu cầu,
// rồi áp hệ số mới (tốc độ, nhịp đánh, idle, màu) và đuổi tiếp.
public class EnemyPhaseChangeState : EnemyState
{
    private const float DefaultDuration = 0.8f; // thiếu clip chuyển phase

    private float timer;

    public EnemyPhaseChangeState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        int phase = enemy.PendingPhase;
        EnemyPhase p = Data.GetPhase(phase);
        enemy.EnterPhase(phase);
        enemy.SetVelocityX(0f);

        float clip = p != null ? enemy.AnimLength(p.transitionAnim) : 0f;
        timer = p != null && p.transitionTime > 0f ? p.transitionTime : (clip > 0f ? clip : DefaultDuration);
        if (p != null)
        {
            enemy.PlayAnim(p.transitionAnim, restart: true);
            if (p.invulnerableDuringTransition)
                enemy.Damageable.SetInvulnerable(timer);
        }
    }

    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        ChangeState(enemy.HasTarget ? (EnemyState)enemy.Chase : enemy.Patrol);
    }

    public override void FixedUpdate()
    {
        enemy.SetVelocityX(0f);
    }
}
