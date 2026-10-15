using System.Collections;
using UnityEngine;

// Cháy: trừ máu theo tick trong suốt thời gian hiệu ứng (đi thẳng vào Health, không qua blocker/popup)
public class BurnEffect : StatusEffect
{
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private int damagePerTick = 5;

    private Health health;

    public override AttackEffect EffectType => AttackEffect.Burn;

    protected override void Awake()
    {
        base.Awake();
        health = GetComponent<Health>();
    }

    protected override IEnumerator Tick(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            health.TakeDamage(damagePerTick);
            yield return new WaitForSeconds(tickInterval);
            timer += tickInterval;
        }
    }
}
