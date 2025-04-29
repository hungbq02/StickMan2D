using UnityEngine;

public class PlayerAnimationAttack : MonoBehaviour
{
    private PlayerController player;
    public Transform bulletSpawnPoint;

    public Transform fighterAttackOrigin;
    public float fighterAttackRange = 1f;

    public Transform swordAttackOrigin;
    public float swordAttackRange = 1.5f;

    public LayerMask enemyLayer;


    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
    }
    public void Fire()
    {
        GameObject bullet = PoolManager.Instance.bulletPool.GetObject();
        Bullet bulletScript = bullet.GetComponent<Bullet>();

        bulletScript.SetDirection(bulletSpawnPoint.right * player.FacingDirection);

        bullet.SetActive(true);
        bullet.transform.position = bulletSpawnPoint.position;
        bullet.transform.rotation = bulletSpawnPoint.rotation;
    }
    public void FighterAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(fighterAttackOrigin.position, fighterAttackRange, enemyLayer);
        AttackDataSO attackData = player.CurrentAttackData;

        foreach (Collider2D collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();

            AttackData attackInfo = new AttackData(
                                    attackData.damage,
                                    attackData.knockbackForce,
                                    attackData.criticalChance,
                                    attackData.criticalMultiplier,
                                    attackData.range,
                                    attackData.effectDuration,
                                    attackData.attackEffect);
            if (enemy != null)
            {
                enemy.TakeDamage(attackInfo);
            }
        }
    }
    public void SwordAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(swordAttackOrigin.position, swordAttackRange, enemyLayer);
        AttackDataSO attackData = player.CurrentAttackData;

        foreach (Collider2D collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                AttackData attackInfo = new AttackData(
                        attackData.damage,
                        attackData.knockbackForce,
                        attackData.criticalChance,
                        attackData.criticalMultiplier,
                        attackData.range,
                        attackData.effectDuration,
                        attackData.attackEffect);
                 enemy.TakeDamage(attackInfo);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(fighterAttackOrigin.position, fighterAttackRange);
        Gizmos.DrawWireSphere(swordAttackOrigin.position, swordAttackRange);
    }

}
