using Kore.Utils.Core;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable, IEffectReceiver
{
    private Rigidbody2D rb;
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private bool isStunned = false;
    private DamagePopup damagePopup;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(AttackData attackData)
    {
        // Tính toán sát thương, có thể là critical hit
        int finalDamage = attackData.damage;
        if (attackData.IsCriticalHit())
        {
            finalDamage = (int) (finalDamage * attackData.criticalMultiplier);
        }

        //damage
        currentHealth -= finalDamage;

        var damagePopup = ObjectPool.Spawn(Service.Get<Bootstrap>().textDamagePrefab,transform.position, Quaternion.identity);
        damagePopup.Init(finalDamage);


        // Áp dụng hiệu ứng (nếu có)
        ApplyEffect(attackData.effectType, attackData.effectDuration);

        if (currentHealth <= 0)
        {
            Die();
        }    
    }
    public void ApplyEffect(AttackEffect effectType, float duration)
    {
        switch (effectType)
        {
            case AttackEffect.None:
                break;

            case AttackEffect.Stun:
                StartCoroutine(ApplyStun(duration));
                break;

            case AttackEffect.Knockback:
                ApplyKnockback();
                break;
        }
    }

    private IEnumerator ApplyStun(float duration)
    {
        if (!isStunned)
        {
            isStunned = true;
            Debug.Log("Enemy Stunned!");

            yield return new WaitForSeconds(duration);

            isStunned = false;
            Debug.Log("Enemy Unstunned!");
        }
    }

    private void ApplyKnockback()
    {
        Debug.Log("Enemy Knocked Back!");

        Vector3 knockbackDirection = (transform.position - Camera.main.transform.position).normalized;
        rb.velocity = knockbackDirection * 3f + Vector3.up * 10f;
    }

    private void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(gameObject, 1f);
    }
}
