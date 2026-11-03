using UnityEngine;

// Đuổi theo player: lên kế hoạch 1 đòn (random theo weight trong các đòn đã hồi) rồi tiến tới tầm của đòn đó.
// Đòn khác dùng được ngay tại chỗ (đòn lùi, bị tường / vực chặn đường, player gần hơn tầm tối thiểu) thì ra luôn.
// Enemy đánh xa (keepAwayDistance > 0): player áp sát thì quay lưng chạy ra rồi quay lại bắn.
// Player đi quá xa => Patrol; player vừa bị hạ => Taunt.
// Mọi ngưỡng đứng / đi / lùi / quay mặt đều có vùng đệm để không nháy clip hoặc lật hướng mỗi frame.
public class EnemyChaseState : EnemyState
{
    private const float FaceDeadZone = 0.3f;     // player lệch ngang < khoảng này (đứng sát, nhảy qua đầu) => giữ hướng
    private const float StopFraction = 0.85f;    // đứng lại khi vào tới 85% tầm đòn định dùng...
    private const float ResumeFraction = 1.0f;   // ...và chỉ đi tiếp khi player ra ngoài hẳn tầm
    private const float RetreatHysteresis = 1f;  // đang lùi: chạy tới keepAway + 1u mới thôi
    private const float MinDwell = 0.35f;        // giữ đứng / đi tối thiểu chừng này giây (player nhích qua lại ngưỡng)

    private bool retreating;
    private bool cornered; // đang lùi mà gặp tường / vực => đứng đánh tới khi player lùi ra
    private bool holding;  // đang đứng chờ trong tầm
    private bool stuck;    // FixedUpdate trước không đi tiếp được (tường / vực) dù chưa tới tầm đòn định dùng
    private float dwellUntil;
    private EnemyAttackSO planned;

    public EnemyChaseState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        retreating = false;
        cornered = false;
        holding = false;
        stuck = false;
        dwellUntil = 0f;
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

        UpdateRetreat(dist);
        float targetX = enemy.Target.position.x;
        if (retreating)
            enemy.FaceTowards(2f * enemy.transform.position.x - targetX, FaceDeadZone); // quay lưng lại player
        else
            enemy.FaceTowards(targetX, FaceDeadZone);
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
            // Bị dồn vào góc => đứng lại, quay mặt lại đánh (giữ tới khi player lùi ra, không lật qua lại)
            if (!enemy.MoveForward(speed))
            {
                retreating = false;
                cornered = true;
                Stand();
                return;
            }
            enemy.PlayAnim(enemy.RunAnim);
            return;
        }

        // Đủ gần cho đòn định dùng (hoặc mọi đòn còn cooldown: chờ ở tầm gần nhất) thì không ép sát thêm;
        // đã đứng thì chỉ đi tiếp khi player ra hẳn ngoài tầm
        float desired = planned != null ? planned.maxRange : enemy.ShortestReach;
        float limit = desired * (holding ? ResumeFraction : StopFraction);
        bool wantHold = enemy.TargetDistanceX <= limit;
        if (wantHold != holding && Time.time >= dwellUntil)
        {
            holding = wantHold;
            dwellUntil = Time.time + MinDwell;
        }
        if (holding)
        {
            Stand();
            return;
        }
        stuck = !enemy.MoveForward(speed);
        enemy.PlayAnim(stuck ? enemy.IdleAnim : enemy.RunAnim);
    }

    private void UpdateRetreat(float dist)
    {
        float keep = Data.keepAwayDistance;
        if (keep <= 0f) { retreating = false; return; }
        if (cornered)
        {
            if (dist > keep + RetreatHysteresis) cornered = false; // player đã lùi ra => lại được phép lùi
            retreating = false;
            return;
        }
        retreating = retreating ? dist < keep + RetreatHysteresis : dist < keep;
    }

    private void Stand()
    {
        stuck = false;
        enemy.SetVelocityX(0f);
        enemy.PlayAnim(enemy.IdleAnim);
    }
}
