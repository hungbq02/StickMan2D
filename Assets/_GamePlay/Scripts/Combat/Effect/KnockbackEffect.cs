using UnityEngine;

[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackEffect : MonoBehaviour
{
    private Rigidbody2D rb;
    private Damageable damageable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        damageable = GetComponent<Damageable>();
        damageable.OnDamaged += OnDamaged;
    }

    private void OnDestroy()
    {
        damageable.OnDamaged -= OnDamaged;
    }

    private void OnDamaged(int damage, AttackData data)
    {
        if (data.knockbackForce <= 0f) return;

        Vector2 dir = ((Vector2)transform.position - data.attackerPosition).normalized;
        rb.velocity = dir * data.knockbackForce;
    }
}
