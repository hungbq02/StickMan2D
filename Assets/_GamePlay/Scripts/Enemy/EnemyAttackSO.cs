using UnityEngine;

public enum EnemyAttackMovement
{
    None,
    Lunge,     // lao tới theo hướng mặt (charge)
    Retreat,   // lùi ra xa mục tiêu (dash lùi của cung thủ)
    Teleport,  // biến mất rồi hiện ra phía sau mục tiêu
}

public enum ProjectileOrigin
{
    Muzzle,      // từ điểm ra đạn, bay theo hướng mặt
    AboveTarget, // rơi thẳng xuống quanh mục tiêu (mưa tên / thiên thạch)
}

// 1 đòn / hành động của enemy: Telegraph (báo hiệu) -> Active (các mốc trúng) -> Recovery.
// EnemyController chọn ngẫu nhiên theo weight trong các đòn hợp lệ (tầm, phase, cooldown).
[CreateAssetMenu(fileName = "NewEnemyAttack", menuName = "Data/Enemy Attack")]
public class EnemyAttackSO : ScriptableObject
{
    [Header("Animation (tên ngắn: 'smash' => state smash_{animSet})")]
    [Tooltip("Tư thế báo hiệu (để trống = giữ animation đứng yên)")]
    public string telegraphAnim;
    public string attackAnim;

    [Header("Timing (giây)")]
    [Tooltip("Thời gian báo hiệu trước khi ra đòn — player dựa vào đây để né")]
    public float telegraphTime = 0.4f;
    [Tooltip("Tô màu sprite trong lúc báo hiệu (windupTint của EnemyController)")]
    public bool tintTelegraph = true;
    [Tooltip("Các mốc gây damage / bắn / sinh vùng nguy hiểm, tính từ lúc bắt đầu animation đòn")]
    public float[] hitTimes = { 0.2f };
    [Tooltip("Độ dài pha ra đòn; 0 = theo độ dài clip attackAnim (không có clip => mốc trúng cuối)")]
    public float activeDuration;
    [Tooltip("Đứng yên sau đòn (player phản công)")]
    public float recoveryTime = 0.35f;
    [Tooltip("Chờ tối thiểu trước khi dùng lại đúng đòn này (tính từ mốc trúng đầu tiên)")]
    public float cooldown = 1.2f;

    [Header("Chọn đòn")]
    [Tooltip("Khoảng cách X tới mục tiêu để dùng được đòn")]
    public float minRange;
    public float maxRange = 1.2f;
    [Min(0f)] public float weight = 1f;
    [Tooltip("Chỉ dùng từ phase này (0 = phase đầu; enrage / boss phase 2 = 1...)")]
    public int minPhase;
    public int maxPhase = 99;

    [Header("Damage cận chiến (để trống = đòn không chém)")]
    public AttackDataSO damage;
    [Tooltip("Tên child Hitbox của enemy; trống = attackHitbox mặc định")]
    public string hitbox;

    [Header("Di chuyển trong pha ra đòn")]
    public EnemyAttackMovement movement;
    [Tooltip("Lunge / Retreat: tốc độ (u/s). Teleport: khoảng cách hiện ra sau lưng mục tiêu")]
    public float moveSpeed = 8f;
    [Tooltip("Khoảng thời gian di chuyển, tính từ lúc bắt đầu animation đòn (Teleport dùng moveStart)")]
    public float moveStart;
    public float moveEnd = 0.3f;
    [Tooltip("Lunge: dừng lao khi mục tiêu ở ngay phía trước trong khoảng này (không xuyên qua người)")]
    public float lungeStopDistance = 0.8f;

    [Header("Đạn (EnemyShooter) — bắn ở mỗi mốc trúng")]
    public EnemyProjectile projectile;
    public AttackDataSO projectileDamage;
    [Min(1)] public int projectileCount = 1;
    [Tooltip("Muzzle: góc tỏa (độ) giữa viên đầu và viên cuối")]
    public float spreadAngle;
    public ProjectileOrigin projectileOrigin;
    [Tooltip("Tên child điểm ra đạn; trống = muzzle mặc định của EnemyShooter")]
    public string muzzle;
    [Tooltip("AboveTarget: rơi từ độ cao này trên đầu mục tiêu, rải đều trên bề rộng rainWidth")]
    public float rainHeight = 6f;
    public float rainWidth = 3f;

    // Hết pha ra đòn: activeDuration > 0 thì theo nó, không thì theo clip; luôn >= mốc trúng cuối
    public float ResolveActiveDuration(float clipLength)
    {
        float lastHit = hitTimes != null && hitTimes.Length > 0 ? hitTimes[hitTimes.Length - 1] : 0f;
        float d = activeDuration > 0f ? activeDuration : clipLength;
        return Mathf.Max(d, lastHit);
    }

    public bool IsUsableAt(float distanceX, int phase)
    {
        return phase >= minPhase && phase <= maxPhase && distanceX >= minRange && distanceX <= maxRange;
    }
}
