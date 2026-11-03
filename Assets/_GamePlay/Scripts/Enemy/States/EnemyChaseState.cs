// Đuổi theo player: lên kế hoạch 1 đòn (random theo weight trong các đòn đã hồi) rồi tiến tới tầm của đòn đó.
// Đòn khác dùng được ngay tại chỗ (đòn lùi, bị tường / vực chặn đường, player gần hơn tầm tối thiểu) thì ra luôn.
// Enemy đánh xa (keepAwayDistance > 0): player áp sát thì quay lưng chạy ra rồi quay lại bắn.
// Player đi quá xa => Patrol; player vừa bị hạ => Taunt.
public class EnemyChaseState : EnemyState
{
    private bool retreating;
    private bool stuck; // FixedUpdate trước không đi tiếp được (tường / vực) dù chưa tới tầm đòn định dùng
    private EnemyAttackSO planned;

    public EnemyChaseState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        retreating = false;
        stuck = false;
        planned = null;
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

        float dist = enemy.TargetDistanceX;
        if (planned == null || !enemy.IsOffCooldown(planned) || dist < planned.minRange
            || enemy.Phase < planned.minPhase || enemy.Phase > planned.maxPhase)
            planned = enemy.PlanAttack();

        if (planned != null && planned.IsUsableAt(dist, enemy.Phase) && enemy.IsAttackReady(planned))
        {
            enemy.StartAttack(planned);
            return;
        }
        EnemyAttackSO now = enemy.ChooseAttack();
        if (now != null && (planned == null || stuck || now.movement == EnemyAttackMovement.Retreat || dist < planned.minRange))
        {
            enemy.StartAttack(now);
            return;
        }

        retreating = Data.keepAwayDistance > 0f && dist < Data.keepAwayDistance;
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

        // Đủ gần cho đòn định dùng (hoặc mọi đòn còn cooldown: chờ ở tầm gần nhất) thì không ép sát thêm
        float desired = planned != null ? planned.maxRange : enemy.ShortestReach;
        if (enemy.TargetDistanceX <= desired * 0.9f)
        {
            Stand();
            return;
        }
        stuck = !enemy.MoveForward(speed);
        enemy.PlayAnim(stuck ? enemy.IdleAnim : enemy.RunAnim);
    }

    private void Stand()
    {
        stuck = false;
        enemy.SetVelocityX(0f);
        enemy.PlayAnim(enemy.IdleAnim);
    }
}
