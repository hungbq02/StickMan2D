using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 direction;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f; 
    public AttackDataSO attackData;
    public LayerMask enemyLayer;
    private PlayerController player;
    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }
    private void OnEnable()
    {
        StartCoroutine(ReturnToPoolAfterTime(lifeTime));
    }

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
            PoolManager.Instance.bulletPool.ReturnObject(gameObject);
        }
    }
    private IEnumerator ReturnToPoolAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        PoolManager.Instance.bulletPool.ReturnObject(gameObject);
    }
}
