using UnityEngine;

public class Shield : MonoBehaviour, IDamageable
{
    [SerializeField] private Health shieldHealth;
    [SerializeField] private Damageable ownerDamageable;

    public void TakeDamage(AttackData data)
    {
        shieldHealth.TakeDamage(data.damage);

        if (!shieldHealth.IsAlive)
        {
            gameObject.SetActive(false);
        }
    }
}
