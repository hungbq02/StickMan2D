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

    public void Fire(int attackStep)
    {
        if (!TryCreateAttackData(WeaponType.Gun, attackStep, out AttackData attackInfo)) return;

        WeaponDataSO weapon = player.WeaponAttackManager.GetWeaponData(WeaponType.Gun);
        Bullet prefab = weapon != null && weapon.projectile != null ? weapon.projectile : Service.Get<PrefabRegistrySO>().bullet;
        Bullet bullet = ObjectPool.Spawn(prefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        bullet.Launch(bulletSpawnPoint.right * player.FacingDirection, attackInfo);
    }

    public void FighterAttack(int attackStep) => CheckHit(fighterHitbox, WeaponType.Fighter, attackStep);

    public void SwordAttack(int attackStep) => CheckHit(swordHitbox, WeaponType.Sword, attackStep);

    public void FighterAirAttack() => CheckHit(fighterAirHitbox, WeaponType.Fighter, WeaponAttackManager.AirAttackStep);

    private void CheckHit(Hitbox hitbox, WeaponType weaponType, int attackStep)
    {
        if (hitbox == null) return;
        if (!TryCreateAttackData(weaponType, attackStep, out AttackData attackInfo)) return;

        hitbox.CheckHit(attackInfo, player.FacingDirection);
    }

    private bool TryCreateAttackData(WeaponType weaponType, int attackStep, out AttackData attackInfo)
    {
        AttackDataSO attackDataSO = player.WeaponAttackManager.GetAttackData(weaponType, attackStep);
        if (attackDataSO == null)
        {
            Debug.LogWarning($"[PlayerAnimationAttack] Không có AttackData cho {weaponType} step {attackStep}", this);
            attackInfo = default;
            return false;
        }

        attackInfo = new AttackData(attackDataSO, player.transform.position);
        attackInfo.sourceTeam = player.Damageable != null ? player.Damageable.Team : Team.Player;
        return true;
    }
}
