using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 direction;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f;
    public LayerMask enemyLayer;

    // Do người bắn truyền vào lúc bắn (crit đã roll sẵn) — prefab không giữ data đòn đánh
    private AttackData attackData;
    private bool hasAttackData;
    private float aliveTime;

    private void OnEnable()
    {
        aliveTime = 0f;
    }

    private void OnDisable()
    {
        hasAttackData = false; // tránh object pool tái dùng data của lần bắn trước
    }

    public void Launch(Vector2 dir, AttackData data)
    {
        direction = dir.normalized;
        attackData = data;
        hasAttackData = true;
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Trả về pool khi bay quá lifeTime mà không trúng gì
        aliveTime += Time.deltaTime;
        if (aliveTime >= lifeTime)
        {
            ObjectPool.Recycle(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            IDamageable enemy = other.GetComponent<IDamageable>();
            if (enemy != null && hasAttackData)
            {
                // Knockback đẩy theo hướng đạn bay => tính từ vị trí viên đạn lúc trúng
                AttackData hit = attackData;
                hit.attackerPosition = transform.position;
                enemy.TakeDamage(hit);
            }
            ObjectPool.Recycle(gameObject);
        }
    }
}
