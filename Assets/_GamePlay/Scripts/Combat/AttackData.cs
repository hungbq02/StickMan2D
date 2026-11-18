using UnityEngine;

public struct AttackData
{
    public int damage;
    public float knockbackForce;
    public float criticalChance;
    public float criticalMultiplier;
    public float range;
    public AttackEffect effectType;
    public float effectDuration;
    public Vector2 attackerPosition;
    // Phe người tấn công — Damageable cùng phe bỏ qua đòn (không friendly fire). Mặc định Neutral.
    public Team sourceTeam;
    // Roll 1 lần lúc tạo đòn đánh (phía người tấn công) => mọi mục tiêu trúng cùng kết quả, UI/VFX biết là crit
    public bool isCritical;
    // Hiệu ứng trúng đòn theo vũ khí / đòn (null = hiệu ứng mặc định trong PrefabRegistrySO)
    public HitFx hitFx;
    public float hitFxScale;
    public float shake; // rung màn hình khi trúng (CameraShake trauma)

    public int FinalDamage => isCritical ? Mathf.RoundToInt(damage * criticalMultiplier) : damage;

    public AttackData(int damage, float knockbackForce, float criticalChance, float criticalMultiplier, float range, float effectDuration, AttackEffect attackEffect, Vector2 attackerPosition)
    {
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.criticalChance = criticalChance;
        this.criticalMultiplier = criticalMultiplier;
        this.range = range;
        this.effectDuration = effectDuration;
        this.effectType = attackEffect;
        this.attackerPosition = attackerPosition;
        this.sourceTeam = Team.Neutral;
        this.isCritical = RollCritical(criticalChance);
        this.hitFx = null;
        this.hitFxScale = 1f;
        this.shake = 0f;
    }

    public AttackData(AttackDataSO dataSO, Vector2 attackerPosition)
        : this(dataSO.damage,
               dataSO.knockbackForce,
               dataSO.criticalChance,
               dataSO.criticalMultiplier,
               dataSO.range,
               dataSO.effectDuration,
               dataSO.attackEffect,
               attackerPosition)
    {
        hitFx = dataSO.hitFx;
        hitFxScale = dataSO.hitFxScale > 0f ? dataSO.hitFxScale : 1f;
        shake = dataSO.shake;
    }

    private static bool RollCritical(float chance)
    {
        return chance > 0f && Random.value <= chance;
    }
}
