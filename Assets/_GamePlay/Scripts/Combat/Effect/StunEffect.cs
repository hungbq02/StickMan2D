using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Damageable))]
public class StunEffect : MonoBehaviour
{
    private Damageable damageable;
    private Coroutine stunCoroutine;

    public bool IsStunned { get; private set; }

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
        damageable.OnDamaged += OnDamaged;
    }

    private void OnDestroy()
    {
        damageable.OnDamaged -= OnDamaged;
    }

    private void OnDamaged(int damage, AttackData data)
    {
        if (data.effectType != AttackEffect.Stun) return;

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(StunRoutine(data.effectDuration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        IsStunned = true;
        yield return new WaitForSeconds(duration);
        IsStunned = false;
    }
}
