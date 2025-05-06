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

    public AttackData(int damage, float knockbackForce, float criticalChance, float criticalMultiplier, float range, float effectDuration, AttackEffect attackEffect)
    {
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.criticalChance = criticalChance;
        this.criticalMultiplier = criticalMultiplier;
        this.range = range;
        this.effectDuration = effectDuration;
        this.effectType = attackEffect;
    }
    public bool IsCriticalHit()
    {
        return Random.Range(0f, 1f) <= criticalChance;
    }
}

