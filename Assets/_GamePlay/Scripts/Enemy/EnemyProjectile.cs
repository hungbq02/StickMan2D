using System.Collections.Generic;
using UnityEngine;

// Đạn của enemy (mũi tên, tia...): bay thẳng theo hướng bắn, gây damage khi chạm targetLayer,
// gặp tường / đất thì biến mất. pierce = xuyên qua mục tiêu (mỗi mục tiêu trúng 1 lần).
// Cần Rigidbody2D Kinematic + Collider2D trigger để nhận va chạm với collider tĩnh (tường).
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask targetLayer;
    [Tooltip("Gặp layer này (đất, tường) thì biến mất")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private bool pierce;
    [Tooltip("Xoay sprite theo hướng bay (sprite vẽ hướng sang phải)")]
    [SerializeField] private bool rotateToDirection = true;

    private Vector2 direction = Vector2.right;
    private AttackData attackData;
    private bool armed;
    private float aliveTime;
    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    public float Speed => speed;

    private void OnEnable()
    {
        aliveTime = 0f;
    }

    private void OnDisable()
    {
        armed = false; // pool tái dùng không mang data lần bắn trước
        hitTargets.Clear();
    }

    public void Launch(Vector2 dir, AttackData data)
    {
        direction = dir.sqrMagnitude > 0f ? dir.normalized : Vector2.right;
        attackData = data;
        armed = true;
        if (rotateToDirection)
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void Update()
    {
        transform.Translate(direction * (speed * Time.deltaTime), Space.World);
        aliveTime += Time.deltaTime;
        if (aliveTime >= lifetime)
            Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!armed) return;
        int bit = 1 << other.gameObject.layer;

        if ((bit & targetLayer) != 0)
        {
            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target != null && hitTargets.Add(target))
            {
                // Knockback đẩy theo hướng đạn bay
                AttackData hit = attackData;
                hit.attackerPosition = (Vector2)transform.position - direction * 0.5f;
                target.TakeDamage(hit);
                if (!pierce) Despawn();
            }
            return;
        }

        if ((bit & obstacleLayer) != 0)
            Despawn();
    }

    private void Despawn()
    {
        armed = false;
        if (ObjectPool.IsSpawned(gameObject))
            ObjectPool.Recycle(gameObject);
        else
            gameObject.SetActive(false);
    }
}
