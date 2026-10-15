using System.Collections.Generic;
using UnityEngine;

public enum HitboxType
{
    OverlapCircle,
    CircleCast
}

public class Hitbox : MonoBehaviour
{
    [SerializeField] private HitboxType hitboxType = HitboxType.OverlapCircle;
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private LayerMask targetLayer;

    [Header("Only for CircleCast")]
    [SerializeField] private Vector2 castDirection = new Vector2(1f, -0.3f);
    [SerializeField] private float castDistance = 2f;

    // Buffer tái sử dụng => không cấp phát mảng mới mỗi đòn
    private readonly List<Collider2D> overlapResults = new List<Collider2D>();
    private readonly List<RaycastHit2D> castResults = new List<RaycastHit2D>();
    // Mỗi IDamageable chỉ nhận damage 1 lần / đòn, kể cả khi có nhiều collider bị trúng
    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    public void CheckHit(AttackData attackData, int facingDirection = 1)
    {
        hitTargets.Clear();
        ContactFilter2D filter = CreateFilter();

        if (hitboxType == HitboxType.OverlapCircle)
        {
            Physics2D.OverlapCircle(transform.position, radius, filter, overlapResults);
            foreach (Collider2D collider in overlapResults)
            {
                TryHit(collider, attackData);
            }
        }
        else if (hitboxType == HitboxType.CircleCast)
        {
            Physics2D.CircleCast(transform.position, radius, GetCastDirection(facingDirection), filter, castResults, castDistance);
            foreach (RaycastHit2D hit in castResults)
            {
                TryHit(hit.collider, attackData);
            }
        }
    }

    private ContactFilter2D CreateFilter()
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(targetLayer);
        filter.useTriggers = Physics2D.queriesHitTriggers; // giữ hành vi như OverlapCircleAll/CircleCastAll
        return filter;
    }

    // Điều chỉnh hướng cast theo hướng mặt của Player
    private Vector2 GetCastDirection(int facingDirection)
    {
        return new Vector2(facingDirection, castDirection.y).normalized;
    }

    private void TryHit(Collider2D collider, AttackData attackData)
    {
        IDamageable iDamageable = collider.GetComponent<IDamageable>();
        if (iDamageable != null && hitTargets.Add(iDamageable))
        {
            iDamageable.TakeDamage(attackData);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);

        if (hitboxType == HitboxType.CircleCast)
        {
            int facing = transform.right.x >= 0 ? 1 : -1;
            Vector3 castEnd = transform.position + (Vector3)(GetCastDirection(facing) * castDistance);
            Gizmos.DrawLine(transform.position, castEnd);
            Gizmos.DrawWireSphere(castEnd, radius);
        }
    }
}
