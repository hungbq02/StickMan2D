using System;
using UnityEngine;

// Máu & damage dùng int thống nhất (AttackData.FinalDamage, DamagePopup, BurnEffect)
public class Health : MonoBehaviour
{
    public event Action<int, int> OnHealthChanged; // current, max
    public event Action OnDeath;

    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    public int Current => currentHealth;
    public int Max => maxHealth;
    public bool IsAlive => currentHealth > 0;

    // OnEnable thay vì Awake: object lấy lại từ ObjectPool sẽ được hồi đầy máu
    private void OnEnable()
    {
        currentHealth = maxHealth;
    }

    // Cho data-driven (vd: EnemyDataSO) ghi đè máu tối đa lúc khởi tạo
    public void SetMaxHealth(int max, bool refill = true)
    {
        maxHealth = Mathf.Max(1, max);
        currentHealth = refill ? maxHealth : Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (!IsAlive || amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
