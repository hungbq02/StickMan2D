// Đuổi theo player tới tầm đánh (EngageRange); có đòn dùng được => Attack.
// Enemy đánh xa (keepAwayDistance > 0): player áp sát thì quay lưng chạy ra rồi quay lại bắn.
// Player đi quá xa => Patrol; player vừa bị hạ => Taunt.
public class EnemyChaseState : EnemyState
{
    private bool retreating;

    public EnemyChaseState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        retreating = false;
    }

    public override void Update()
    {
        if (!enemy.CanAct) return;

        if (!enemy.HasTarget || enemy.TargetDistanceX > Data.loseRange || !enemy.TargetSameLevel)
        {
            if (enemy.TargetDefeated && enemy.Anim != null && enemy.Anim.Has(Data.animTaunt))
                ChangeState(enemy.Taunt);
            else
                ChangeState(enemy.Patrol);
            return;
        }

        EnemyAttackSO attack = enemy.ChooseAttack();
        if (attack != null)
        {
            enemy.StartAttack(attack);
            return;
        }

        retreating = Data.keepAwayDistance > 0f && enemy.TargetDistanceX < Data.keepAwayDistance;
        float targetX = enemy.Target.position.x;
        if (retreating)
            enemy.FaceTowards(2f * enemy.transform.position.x - targetX); // quay lưng lại player
        else
            enemy.FaceTowards(targetX);
    }

    public override void FixedUpdate()
    {
        if (!enemy.CanAct || !enemy.HasTarget)
        {
            Stand();
            return;
        }

        float speed = Data.chaseSpeed * enemy.SpeedMultiplier;
        if (retreating)
        {
            // Bị dồn vào góc => đứng lại (Update sẽ quay mặt lại đánh)
            if (!enemy.MoveForward(speed))
            {
                retreating = false;
                Stand();
                return;
            }
            enemy.PlayAnim(enemy.RunAnim);
            return;
        }

        // Đã trong tầm đánh (đang chờ cooldown) thì không ép sát thêm
        if (enemy.TargetDistanceX <= enemy.EngageRange * 0.9f)
        {
            Stand();
            return;
        }
        if (enemy.MoveForward(speed))
            enemy.PlayAnim(enemy.RunAnim);
        else
            enemy.PlayAnim(enemy.IdleAnim);
    }

    private void Stand()
    {
        enemy.SetVelocityX(0f);
        enemy.PlayAnim(enemy.IdleAnim);
    }
}
