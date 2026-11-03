using System.Collections.Generic;
using UnityEngine;

public enum EnemyFlinch
{
    Always,            // trúng đòn là khựng (Hurt), kể cả đang vung
    NotWhileAttacking, // đang ra đòn thì không bị ngắt (elite)
    Never,             // không bao giờ khựng (boss) — knockback / effect vẫn tác dụng
}

// Phase theo % máu (enrage của elite, các phase của boss). Phase 0 = mặc định, không cần khai báo.
[System.Serializable]
public class EnemyPhase
{
    [Tooltip("Vào phase khi máu <= tỉ lệ này (0..1)")]
    [Range(0f, 1f)] public float healthFraction = 0.5f;
    [Tooltip("Animation chuyển phase (enrage / phase / roar); trống = chuyển ngay")]
    public string transitionAnim = "enrage";
    [Tooltip("Thời gian chuyển phase; 0 = theo độ dài clip")]
    public float transitionTime;
    [Tooltip("Bất tử trong lúc chuyển phase")]
    public bool invulnerableDuringTransition = true;
    [Tooltip("Thay animation đứng yên (vd rageIdle); trống = giữ")]
    public string idleAnim;
    public float moveSpeedMultiplier = 1.2f;
    [Tooltip("Nhân thời gian báo hiệu + cooldown (< 1 = đánh dồn hơn)")]
    public float tempoMultiplier = 0.8f;
    public Color tint = Color.white;
}

// Chỉ số + hành vi của 1 loại enemy (EnemyController đọc). Mỗi loại enemy = 1 asset.
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Data/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Stats")]
    public int maxHealth = 50;

    [Header("Movement")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3f;
    [Tooltip("Đi tuần qua lại quanh vị trí xuất phát ± khoảng này")]
    public float patrolDistance = 3f;
    [Tooltip("Thời gian đứng nghỉ ở mỗi đầu đường tuần tra")]
    public float patrolPause = 0.6f;
    [Tooltip("> 0: đánh xa — player lại gần hơn khoảng này thì lùi ra")]
    public float keepAwayDistance;

    [Header("Detection")]
    [Tooltip("Thấy player trong khoảng này (theo trục X) => đuổi")]
    public float detectRange = 6f;
    [Tooltip("Player ra ngoài khoảng này => bỏ đuổi, quay về tuần tra")]
    public float loseRange = 9f;
    [Tooltip("Chênh lệch độ cao tối đa để còn coi là thấy / đánh được")]
    public float maxHeightDifference = 1.5f;

    [Header("Attack")]
    public List<EnemyAttackSO> attacks = new List<EnemyAttackSO>();
    [Tooltip("Nghỉ tối thiểu giữa 2 đòn bất kỳ (ngoài cooldown riêng từng đòn)")]
    public float attackInterval = 0.4f;

    [Header("Hurt / Death")]
    public EnemyFlinch flinch = EnemyFlinch.Always;
    [Tooltip("Thời gian khựng lại khi bị đánh")]
    public float hurtDuration = 0.25f;
    [Tooltip("Xác nằm lại sau khi animation chết xong (mờ dần) rồi mới về pool / tắt")]
    public float corpseLinger = 0.6f;

    [Header("Phases (theo % máu, xếp giảm dần)")]
    public List<EnemyPhase> phases = new List<EnemyPhase>();

    [Header("Animation (tên ngắn, xem EnemyAnimator)")]
    public string animIdle = "idle";
    public string animWalk = "walk";
    [Tooltip("Lúc đuổi; trống / thiếu clip => dùng animWalk")]
    public string animRun = "run";
    public string animHit = "hit";
    public string animDeath = "death";
    [Tooltip("Phát 1 lần khi giết được player; trống = bỏ qua")]
    public string animTaunt = "taunt";

    // Phase ứng với tỉ lệ máu hiện tại: số phase đã vượt ngưỡng (phases xếp theo healthFraction giảm dần)
    public int PhaseForHealth(float healthFraction)
    {
        int phase = 0;
        for (int i = 0; i < phases.Count; i++)
            if (healthFraction <= phases[i].healthFraction) phase = i + 1;
        return phase;
    }

    public EnemyPhase GetPhase(int phase) => phase > 0 && phase <= phases.Count ? phases[phase - 1] : null;
}
