using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    private Rigidbody2D rb;
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private bool isStunned = false;

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
        DamagePopupManager.Instance.DisplayDamagePopup(finalDamage, transform);

    //    Debug.Log("Enemy Health: " + currentHealth);

        // Áp dụng hiệu ứng (nếu có)
        ApplyEffect(attackData.effectType, attackData.effectDuration);

        if (currentHealth <= 0)
        {
            Die();
        }    
    }
    private void ApplyEffect(AttackEffect effectType, float effectDuration)
    {
        switch (effectType)
        {
            case AttackEffect.None:
                break;
            case AttackEffect.Stun:
                StartCoroutine(ApplyStun(effectDuration));
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

            // Giả sử enemy bị stun trong thời gian duration
            yield return new WaitForSeconds(duration);

            isStunned = false;
            Debug.Log("Enemy Unstunned!");
        }
    }


    // Hiệu ứng Knockback - đẩy đối thủ ra ngoài
    private void ApplyKnockback()
    {    
        Debug.Log("Enemy Knocked Back!");
        Vector3 knockbackDirection = transform.position - Camera.main.transform.position;
        rb.velocity = knockbackDirection * 3f + Vector3.up * 10f;
    }

    private void Die()
    {

        Debug.Log("Enemy died!");
        Destroy(gameObject, 1f);
    }
}
