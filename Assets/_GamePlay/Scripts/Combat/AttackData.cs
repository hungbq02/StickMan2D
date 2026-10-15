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
    // Roll 1 lần lúc tạo đòn đánh (phía người tấn công) => mọi mục tiêu trúng cùng kết quả, UI/VFX biết là crit
    public bool isCritical;

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
        this.isCritical = RollCritical(criticalChance);
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
    }

    private static bool RollCritical(float chance)
    {
        return chance > 0f && Random.value <= chance;
    }
}
