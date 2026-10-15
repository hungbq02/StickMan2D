using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Damageable))]
public class BurnEffect : MonoBehaviour
{
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float damagePerTick = 5f;

    private Health health;
    private Damageable damageable;
    private Coroutine burnCoroutine;

    private void Awake()
    {
        health = GetComponent<Health>();
        damageable = GetComponent<Damageable>();
        damageable.OnDamaged += OnDamaged;
    }

    private void OnDestroy()
    {
        damageable.OnDamaged -= OnDamaged;
    }

    private void OnDamaged(int damage, AttackData data)
    {
        if (data.effectType != AttackEffect.Burn) return;

        Apply(data.effectDuration, damagePerTick);
    }

    public void Apply(float duration, float dmgPerTick)
    {
        // Trúng burn mới thì làm mới thời gian cháy
        if (burnCoroutine != null)
            StopCoroutine(burnCoroutine);

        burnCoroutine = StartCoroutine(BurnRoutine(duration, dmgPerTick));
    }

    private IEnumerator BurnRoutine(float duration, float dmg)
    {
        float timer = 0f;
        while (timer < duration)
        {
            health.TakeDamage(dmg);
            yield return new WaitForSeconds(tickInterval);
            timer += tickInterval;
        }
        burnCoroutine = null;
    }
}
