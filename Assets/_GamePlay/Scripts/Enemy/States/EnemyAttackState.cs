using UnityEngine;

// Đứng lại, vung đòn (attackWindup, sprite tô màu báo hiệu) -> gây damage qua Hitbox -> hồi (attackRecovery) -> Chase
public class EnemyAttackState : EnemyState
{
    private float timer;
    private bool hasStruck;

    public EnemyAttackState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        hasStruck = false;
        timer = Data.attackWindup;
        enemy.SetVelocityX(0f);
        if (enemy.HasTarget)
            enemy.FaceTowards(enemy.Target.position.x);
        enemy.SetWindupTint(true);
    }

    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (!hasStruck)
        {
            hasStruck = true;
            enemy.SetWindupTint(false);
            enemy.PerformAttack();
            timer = Data.attackRecovery;
            return;
        }

        ChangeState(enemy.Chase);
    }

    public override void FixedUpdate()
    {
        enemy.SetVelocityX(0f);
    }

    public override void Exit()
    {
        // Bị ngắt giữa lúc vung (Hurt) => trả màu và tính như đã ra đòn để không đánh lại ngay
        enemy.SetWindupTint(false);
        if (!hasStruck)
            enemy.LastAttackTime = Time.time;
    }
}
