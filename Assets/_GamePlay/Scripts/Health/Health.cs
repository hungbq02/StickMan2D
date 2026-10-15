using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action OnDeath;

    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public float Current => currentHealth;
    public float Max => maxHealth;
    public bool IsAlive => currentHealth > 0f;

    // OnEnable thay vì Awake: object lấy lại từ ObjectPool sẽ được hồi đầy máu
    private void OnEnable()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive || amount <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        if (!IsAlive || amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
