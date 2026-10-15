using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 direction;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f;
    public AttackDataSO attackData;
    public LayerMask enemyLayer;

    private float aliveTime;

    private void OnEnable()
    {
        aliveTime = 0f;
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);

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
            if (enemy != null)
            {
                enemy.TakeDamage(new AttackData(attackData, transform.position));
            }
            ObjectPool.Recycle(gameObject);
        }
    }
}
