using UnityEngine;

public class PlayerAnimationAttack : MonoBehaviour
{
    private PlayerController player;
    public Transform bulletSpawnPoint;

    [Header("Fighter Attack")]
    public Transform fighterAttackOrigin;
    public float fighterAttackRange = 0.4f;

    [Header("Fighter Air Attack")]
    public Transform fighterAirAttackOrigin;
    public float fighterAirAttackRange = 0.3f;

    [Header("Sword Attack")]
    public Transform swordAttackOrigin;
    public float swordAttackRange = 0.5f;

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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(fighterAttackOrigin.position, fighterAttackRange, enemyLayer); //Kiểm tra đối tượng hiện có bên trong
        // Lấy AttackData tương ứng với WeaponType
        AttackDataSO attackData = player.weaponAttackManager.CurrentAttackData;

        foreach (Collider2D collider in colliders)
        {
            IDamageable iDamageable  = collider.GetComponent<IDamageable>();

            AttackData attackInfo = new AttackData(
                                    attackData.damage,
                                    attackData.knockbackForce,
                                    attackData.criticalChance,
                                    attackData.criticalMultiplier,
                                    attackData.range,
                                    attackData.effectDuration,
                                    attackData.attackEffect);
            iDamageable?.TakeDamage(attackInfo);
        }
    }
    public void SwordAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(swordAttackOrigin.position, swordAttackRange, enemyLayer); //Kiểm tra va chạm trên đường di chuyển
        // Lấy AttackData tương ứng với WeaponType
        AttackDataSO attackData = player.weaponAttackManager.CurrentAttackData;

        foreach (Collider2D collider in colliders)
        {
            IDamageable iDamageable = collider.GetComponent<IDamageable>();
            if (iDamageable != null)
            {
                AttackData attackInfo = new AttackData(
                        attackData.damage,
                        attackData.knockbackForce,
                        attackData.criticalChance,
                        attackData.criticalMultiplier,
                        attackData.range,
                        attackData.effectDuration,
                        attackData.attackEffect);
                 iDamageable.TakeDamage(attackInfo);
            }
        }
    }
    public void FighterAirAttack()
    {
        // Lấy AttackData tương ứng với WeaponType
        AttackDataSO attackData = player.weaponAttackManager.CurrentAttackData; 

        // Thiết lập hướng đá chéo và khoảng cách quét
        Vector2 origin = fighterAirAttackOrigin.position;
        Vector2 direction = new Vector2(player.FacingDirection, -0.3f).normalized;
        float distance = 2f; // Có thể chỉnh dài hơn nếu tốc độ dive nhanh

        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, fighterAirAttackRange, direction, distance, enemyLayer);

        foreach (RaycastHit2D hit in hits)
        {
            IDamageable iDamageable = hit.collider.GetComponent<IDamageable>();
            if (iDamageable != null)
            {
                AttackData attackInfo = new AttackData(
                    attackData.damage,
                    attackData.knockbackForce,
                    attackData.criticalChance,
                    attackData.criticalMultiplier,
                    attackData.range,
                    attackData.effectDuration,
                    attackData.attackEffect);
                iDamageable.TakeDamage(attackInfo);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(swordAttackOrigin.position, swordAttackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(fighterAttackOrigin.position, fighterAttackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(fighterAirAttackOrigin.position, fighterAirAttackRange);
    }

}
