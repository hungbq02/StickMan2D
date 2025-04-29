using System;
using UnityEngine;

public class HeathSystem : MonoBehaviour
{
    public event Action OnHealthZero;

    [SerializeField] private float maxHealth;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void DecreaseHealth(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            OnHealthZero?.Invoke();

            Debug.Log("Health is zero!!");
        }
    }

    public void IncreaseHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }
}
