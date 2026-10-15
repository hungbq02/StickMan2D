using Kore.Utils.Core;
using UnityEngine;

// Các hàm public được gọi từ Animation Event — không đổi tên.
// Tham số int của event = bước đòn của clip: 0 = đòn trên không, 1..3 = bước combo (xem WeaponAttackManager).
public class PlayerAnimationAttack : MonoBehaviour
{
    private PlayerController player;
    public Transform bulletSpawnPoint;

    [Header("Hitboxes")]
    public Hitbox fighterHitbox;
    public Hitbox fighterAirHitbox;
    public Hitbox swordHitbox;

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
    }

    public void Fire()
    {
        Bullet bullet = ObjectPool.Spawn(Service.Get<Bootstrap>().bulletPrefab,
                                        bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        bullet.SetDirection(bulletSpawnPoint.right * player.FacingDirection);
    }

    public void FighterAttack(int attackStep) => CheckHit(fighterHitbox, WeaponType.Fighter, attackStep);

    public void SwordAttack(int attackStep) => CheckHit(swordHitbox, WeaponType.Sword, attackStep);

    public void FighterAirAttack() => CheckHit(fighterAirHitbox, WeaponType.Fighter, WeaponAttackManager.AirAttackStep);

    private void CheckHit(Hitbox hitbox, WeaponType weaponType, int attackStep)
    {
        if (hitbox == null) return;

        AttackDataSO attackDataSO = player.WeaponAttackManager.GetAttackData(weaponType, attackStep);
        if (attackDataSO == null)
        {
            Debug.LogWarning($"[PlayerAnimationAttack] Không có AttackData cho {weaponType} step {attackStep}", this);
            return;
        }

        AttackData attackInfo = new AttackData(attackDataSO, player.transform.position);
        hitbox.CheckHit(attackInfo, player.FacingDirection);
    }
}
