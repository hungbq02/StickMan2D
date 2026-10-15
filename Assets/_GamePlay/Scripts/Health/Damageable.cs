using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Death))]
public class Damageable : MonoBehaviour, IDamageable
{
    [SerializeField] private Team team = Team.Neutral;
    public Team Team => team;

    private Health health;
    private readonly List<IDamageBlocker> blockers = new List<IDamageBlocker>();

    public event Action<int, AttackData> OnDamaged;
    public event Action<AttackData> OnBlocked;

    // I-frame: trong thời gian này mọi đòn đánh bị bỏ qua (DoT như Burn trừ thẳng Health nên vẫn tác dụng)
    private float invulnerableUntil;
    public bool IsInvulnerable => Time.time < invulnerableUntil;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    // Bắn mỗi lần object được bật lại (vd: Spawn từ ObjectPool) — cho các phần đã tắt (khiên vỡ) tự hồi
    public event Action OnRespawned;

    private void OnEnable()
    {
        invulnerableUntil = 0f; // object lấy lại từ pool không giữ i-frame cũ
        OnRespawned?.Invoke();
    }

    public void SetInvulnerable(float duration)
    {
        invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + duration);
    }

    public void AddBlocker(IDamageBlocker blocker)
    {
        if (!blockers.Contains(blocker)) blockers.Add(blocker);
    }

    public void RemoveBlocker(IDamageBlocker blocker)
    {
        blockers.Remove(blocker);
    }

    private bool IsSameTeam(Team sourceTeam)
    {
        return team != Team.Neutral && sourceTeam == team;
    }

    public void TakeDamage(AttackData attackData)
    {
        if (health == null || !health.IsAlive || IsInvulnerable) return;
        if (IsSameTeam(attackData.sourceTeam)) return;

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

        // Popup, knockback, effect... nghe OnDamaged (vd: DamagePopupSpawner)
        OnDamaged?.Invoke(finalDamage, attackData);
    }
}
