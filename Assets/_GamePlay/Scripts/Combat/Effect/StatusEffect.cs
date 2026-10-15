using System.Collections;
using UnityEngine;

// Base cho hiệu ứng trạng thái (Stun, Burn, Freeze...): nghe Damageable.OnDamaged, tự bật khi đòn đánh mang
// đúng AttackEffect, đếm thời gian, và luôn dọn trạng thái khi object bị tắt (ObjectPool / chết giữa chừng).
[RequireComponent(typeof(Damageable))]
public abstract class StatusEffect : MonoBehaviour
{
    private Damageable damageable;
    private Coroutine routine;

    public abstract AttackEffect EffectType { get; }
    public bool IsActive { get; private set; }

    protected virtual void Awake()
    {
        damageable = GetComponent<Damageable>();
        damageable.OnDamaged += HandleDamaged;
    }

    protected virtual void OnDestroy()
    {
        if (damageable != null)
            damageable.OnDamaged -= HandleDamaged;
    }

    protected virtual void OnDisable()
    {
        Stop();
    }

    private void HandleDamaged(int damage, AttackData data)
    {
        if (data.effectType != EffectType || data.effectDuration <= 0f) return;
        Apply(data.effectDuration);
    }

    // Trúng lại khi đang có hiệu ứng => làm mới thời gian
    public void Apply(float duration)
    {
        if (!isActiveAndEnabled) return;

        if (routine != null)
            StopCoroutine(routine);
        else
            Begin();

        IsActive = true;
        routine = StartCoroutine(Run(duration));
    }

    public void Stop()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        if (IsActive)
        {
            IsActive = false;
            End();
        }
    }

    private IEnumerator Run(float duration)
    {
        yield return Tick(duration);
        routine = null;
        IsActive = false;
        End();
    }

    // Mặc định: chờ hết thời gian. Hiệu ứng theo tick (Burn) override.
    protected virtual IEnumerator Tick(float duration)
    {
        yield return new WaitForSeconds(duration);
    }

    protected virtual void Begin() { }
    protected virtual void End() { }
}
