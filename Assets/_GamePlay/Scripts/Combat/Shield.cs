using UnityEngine;

// Khiên hấp thụ đòn cho Damageable của chủ (không phải mục tiêu riêng => hitbox trúng cả khiên lẫn thân
// cũng chỉ xử lý 1 lần). Hết máu khiên thì tắt GameObject.
[RequireComponent(typeof(Health))]
public class Shield : MonoBehaviour, IDamageBlocker
{
    [Tooltip("Để trống => tự tìm Damageable ở object cha")]
    [SerializeField] private Damageable owner;
    [Tooltip("false = chỉ chặn đòn đến từ phía có khiên (theo vị trí khiên so với chủ)")]
    [SerializeField] private bool blockAllDirections = false;

    private Health shieldHealth;

    private void Awake()
    {
        shieldHealth = GetComponent<Health>();
        if (owner == null)
            owner = GetComponentInParent<Damageable>();
    }

    private void OnEnable()
    {
        if (owner != null) owner.AddBlocker(this);
    }

    private void OnDisable()
    {
        if (owner != null) owner.RemoveBlocker(this);
    }

    public bool TryBlock(AttackData attackData)
    {
        if (!shieldHealth.IsAlive) return false;
        if (!blockAllDirections && !IsOnShieldSide(attackData.attackerPosition)) return false;

        shieldHealth.TakeDamage(attackData.FinalDamage);
        if (!shieldHealth.IsAlive)
        {
            gameObject.SetActive(false);
        }
        return true;
    }

    private bool IsOnShieldSide(Vector2 attackerPosition)
    {
        float ownerX = owner.transform.position.x;
        float shieldSide = transform.position.x - ownerX;
        if (Mathf.Approximately(shieldSide, 0f)) return true; // khiên ở giữa => coi như chặn mọi hướng

        return Mathf.Sign(attackerPosition.x - ownerX) == Mathf.Sign(shieldSide);
    }
}
