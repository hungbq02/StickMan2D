using Kore.Utils.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Death))]
public class Damageable : MonoBehaviour, IDamageable
{
    private Health health;
    private readonly List<IDamageBlocker> blockers = new List<IDamageBlocker>();

    public event Action<int, AttackData> OnDamaged;
    public event Action<AttackData> OnBlocked;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    public void AddBlocker(IDamageBlocker blocker)
    {
        if (!blockers.Contains(blocker)) blockers.Add(blocker);
    }

    public void RemoveBlocker(IDamageBlocker blocker)
    {
        blockers.Remove(blocker);
    }

    public void TakeDamage(AttackData attackData)
    {
        if (health == null || !health.IsAlive) return;

        // Duyệt ngược: blocker có thể tự gỡ (khiên vỡ => SetActive(false)) ngay trong TryBlock
        for (int i = blockers.Count - 1; i >= 0; i--)
        {
            if (blockers[i].TryBlock(attackData))
            {
                OnBlocked?.Invoke(attackData);
                return;
            }
        }

        int finalDamage = attackData.FinalDamage;
        health.TakeDamage(finalDamage);

        var popup = ObjectPool.Spawn(Service.Get<Bootstrap>().textDamagePrefab, transform.position, Quaternion.identity);
        popup.Init(finalDamage);

        OnDamaged?.Invoke(finalDamage, attackData);
    }
}
