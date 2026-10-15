using UnityEngine;

// Khựng lại khi trúng đòn (knockback vẫn tác dụng); hết choáng / đóng băng mới hành động tiếp.
// Bị đánh thì luôn "để ý" player => ưu tiên Chase nếu player còn trong loseRange.
public class EnemyHurtState : EnemyState
{
    private float timer;

    public EnemyHurtState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        timer = Data.hurtDuration;
    }

    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f || !enemy.CanAct) return;

        bool targetNearby = enemy.HasTarget && enemy.TargetSameLevel && enemy.TargetDistanceX <= Data.loseRange;
        ChangeState(targetNearby ? (EnemyState)enemy.Chase : enemy.Patrol);
    }
}
