using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Vùng nguy hiểm trên mặt đất (cột lửa, sóng chấn động...): chờ delay -> báo hiệu (hình mờ nhấp nháy, warnTime)
// -> active (gây damage 1 lần / mục tiêu trong hộp size, activeTime) -> về pool. Bám mặt đất lúc Arm.
public class GroundHazard : MonoBehaviour
{
    [SerializeField] private float warnTime = 0.6f;
    [SerializeField] private float activeTime = 0.5f;
    [Tooltip("Hộp gây damage, tính từ gốc (gốc = điểm chạm đất)")]
    [SerializeField] private Vector2 size = new Vector2(1f, 2f);
    [SerializeField] private Vector2 offset = new Vector2(0f, 1f);
    [SerializeField] private LayerMask targetLayer;
    [Tooltip("Layer mặt đất để bám xuống (0 = giữ nguyên độ cao)")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private SpriteRenderer visual;
    [Tooltip("Animation lúc active (trống = giữ sprite)")]
    [SerializeField] private SpriteSequence sequence;
    [SerializeField] private Color warnColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private float warnPulse = 8f;

    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
    private Coroutine routine;

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        hitTargets.Clear();
    }

    public void Arm(AttackData attackData, float delay)
    {
        SnapToGround();
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run(attackData, delay));
    }

    private void SnapToGround()
    {
        if (groundLayer.value == 0) return;
        Vector2 origin = (Vector2)transform.position + Vector2.up * 3f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 8f, groundLayer);
        if (hit.collider != null)
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
    }

    private IEnumerator Run(AttackData attackData, float delay)
    {
        hitTargets.Clear();
        SetVisible(false);
        if (delay > 0f) yield return new WaitForSeconds(delay);

        // Báo hiệu: frame đầu mờ, nhấp nháy
        SetVisible(true);
        if (sequence != null) { sequence.enabled = false; sequence.ShowFrame(0); }
        for (float t = 0f; t < warnTime; t += Time.deltaTime)
        {
            Color c = warnColor;
            c.a *= 0.6f + 0.4f * Mathf.Sin(t * warnPulse * Mathf.PI * 2f);
            if (visual != null) visual.color = c;
            yield return null;
        }

        // Active: gây damage mỗi frame cho mục tiêu mới trong hộp
        if (visual != null) visual.color = Color.white;
        if (sequence != null) { sequence.enabled = true; sequence.Play(); }
        attackData.attackerPosition = transform.position;
        var filter = new ContactFilter2D();
        filter.SetLayerMask(targetLayer);
        filter.useTriggers = Physics2D.queriesHitTriggers;
        for (float t = 0f; t < activeTime; t += Time.deltaTime)
        {
            Physics2D.OverlapBox((Vector2)transform.position + offset, size, 0f, filter, overlaps);
            foreach (Collider2D c in overlaps)
            {
                IDamageable target = c.GetComponentInParent<IDamageable>();
                if (target != null && hitTargets.Add(target))
                    target.TakeDamage(attackData);
            }
            yield return null;
        }

        routine = null;
        if (ObjectPool.IsSpawned(gameObject)) ObjectPool.Recycle(gameObject);
        else gameObject.SetActive(false);
    }

    private void SetVisible(bool visible)
    {
        if (visual != null) visual.enabled = visible;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f);
        Gizmos.DrawWireCube(transform.position + (Vector3)offset, size);
    }
}
