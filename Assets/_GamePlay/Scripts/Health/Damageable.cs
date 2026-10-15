using Kore.Utils.Core;
using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Death))]
public class Damageable : MonoBehaviour, IDamageable
{
    private Health health;

    public event Action<int, AttackData> OnDamaged;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    public void TakeDamage(AttackData attackData)
    {
        if (health == null || !health.IsAlive) return;

        int finalDamage = attackData.FinalDamage;
        health.TakeDamage(finalDamage);

        var popup = ObjectPool.Spawn(Service.Get<Bootstrap>().textDamagePrefab, transform.position, Quaternion.identity);
        popup.Init(finalDamage);

        OnDamaged?.Invoke(finalDamage, attackData);
    }
}
