using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 direction;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f; 
    public AttackDataSO attackData;
    public LayerMask enemyLayer;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Tạo AttackData từ AttackDataSO
                AttackData attackInfo = new AttackData(
                    attackData.damage,
                    attackData.knockbackForce,
                    attackData.criticalChance,
                    attackData.criticalMultiplier,
                    attackData.range,
                    attackData.effectDuration,
                    attackData.attackEffect);
                // Gọi hàm TakeDamage để gây sát thương cho kẻ thù
                enemy.TakeDamage(attackInfo); // Truyền AttackData vào
            }

            // Quay lại pool hoặc hủy đối tượng đạn
            ObjectPool.Recycle(this.gameObject);
        }
    }
}
