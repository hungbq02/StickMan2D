using Kore.Utils.Core;
using UnityEngine;

// Phản hồi khi trúng đòn: hiệu ứng va chạm (thường / chí mạng / bị chặn) + hit-stop.
// Nghe Damageable.OnDamaged / OnBlocked như DamagePopupSpawner — không gọi trực tiếp từ combat.
[RequireComponent(typeof(Damageable))]
public class HitFeedback : MonoBehaviour
{
    [Header("Hit-stop (giây thật)")]
    [SerializeField] private float hitStop = 0.06f;
    [SerializeField] private float critHitStop = 0.11f;
    [SerializeField] private float blockHitStop = 0.04f;

    [Header("Hiệu ứng")]
    [Tooltip("Cỡ hiệu ứng (theo thân to / nhỏ)")]
    [SerializeField] private float fxScale = 1f;
    [Tooltip("Lệch điểm va chạm về phía đòn đến, theo nửa bề rộng collider (0 = giữa thân)")]
    [SerializeField, Range(0f, 1f)] private float towardAttacker = 0.6f;
    [Tooltip("Độ cao điểm va chạm theo chiều cao collider (0 = chân, 1 = đỉnh đầu)")]
    [SerializeField, Range(0f, 1f)] private float heightRatio = 0.55f;

    private Damageable damageable;
    private Collider2D body;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
        body = GetComponent<Collider2D>();
        damageable.OnDamaged += HandleDamaged;
        damageable.OnBlocked += HandleBlocked;
    }

    private void OnDestroy()
    {
        if (damageable == null) return;
        damageable.OnDamaged -= HandleDamaged;
        damageable.OnBlocked -= HandleBlocked;
    }

    private void HandleDamaged(int damage, AttackData data)
    {
        var reg = Service.Get<PrefabRegistrySO>();
        Spawn(data.isCritical ? reg.critFx : reg.hitFx, data, data.isCritical ? 1.25f : 1f);
        HitStop.Request(data.isCritical ? critHitStop : hitStop);
    }

    private void HandleBlocked(AttackData data)
    {
        Spawn(Service.Get<PrefabRegistrySO>().blockFx, data, 1f);
        HitStop.Request(blockHitStop);
    }

    private void Spawn(HitFx prefab, AttackData data, float extraScale)
    {
        if (prefab == null) return;
        Vector2 point = ImpactPoint(data.attackerPosition, out float dirX);
        var fx = ObjectPool.Spawn(prefab, point, Quaternion.identity);
        fx.Init(dirX, fxScale * extraScale);
    }

    // Điểm va chạm trên thân, lệch về phía người đánh; dirX = hướng đòn bay tới (từ người đánh sang mục tiêu)
    private Vector2 ImpactPoint(Vector2 attacker, out float dirX)
    {
        Bounds b = body != null ? body.bounds : new Bounds(transform.position, Vector3.one);
        float side = attacker.x >= b.center.x ? 1f : -1f;
        dirX = -side;
        float x = b.center.x + side * b.extents.x * towardAttacker;
        float y = b.min.y + b.size.y * heightRatio + Random.Range(-0.12f, 0.12f) * b.size.y;
        return new Vector2(x, y);
    }
}
