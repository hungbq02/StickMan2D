using Kore.Utils.Core;
using UnityEngine;

// Hiện số damage khi Damageable mất máu (tách khỏi Damageable: object nào không cần popup thì không gắn)
[RequireComponent(typeof(Damageable))]
public class DamagePopupSpawner : MonoBehaviour
{
    [SerializeField] private Vector3 offset = Vector3.zero;

    private Damageable damageable;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
        damageable.OnDamaged += HandleDamaged;
    }

    private void OnDestroy()
    {
        if (damageable != null)
            damageable.OnDamaged -= HandleDamaged;
    }

    private void HandleDamaged(int damage, AttackData data)
    {
        var popup = ObjectPool.Spawn(Service.Get<PrefabRegistrySO>().damagePopup, transform.position + offset, Quaternion.identity);
        popup.Init(damage);
    }
}
