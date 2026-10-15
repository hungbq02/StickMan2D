using Kore.Utils.Core;
using UnityEngine;

// FighterAttack / SwordAttack / FighterAirAttack được gọi từ Animation Event — không đổi tên.
// Tham số int của event = bước đòn của clip: 0 = đòn trên không, 1..3 = bước combo (xem WeaponAttackManager).
// Súng: FireProjectile gọi từ code (xem chú thích bên dưới).
public class PlayerAnimationAttack : MonoBehaviour
{
    private PlayerController player;
    [Tooltip("Đầu nòng ở frame bắn của attack_Gun (đuôi viên đạn trùng đầu nòng)")]
    public Transform bulletSpawnPoint;
    [Tooltip("Đầu nòng ở frame bắn của airAttack_Gun — để trống => dùng bulletSpawnPoint")]
    public Transform airBulletSpawnPoint;

    [Header("Hitboxes")]
    public Hitbox fighterHitbox;
    public Hitbox fighterAirHitbox;
    public Hitbox swordHitbox;

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
    }

    // Gọi từ code (GunAttackHandler / AirAttackState) ngay lúc phát animation bắn, KHÔNG qua animation event:
    // event ở đầu clip không chắc bắn lại khi phát lại cùng state => có lúc ra animation mà không ra đạn.
    // Animator cập nhật trong cùng frame trước khi render => đạn và frame giật súng hiện cùng lúc.
    public void FireProjectile(int attackStep)
    {
        if (!TryCreateAttackData(WeaponType.Gun, attackStep, out AttackData attackInfo)) return;

        WeaponDataSO weapon = player.WeaponAttackManager.GetWeaponData(WeaponType.Gun);
        Bullet prefab = weapon != null && weapon.projectile != null ? weapon.projectile : Service.Get<PrefabRegistrySO>().bullet;

        Transform muzzle = attackStep == WeaponAttackManager.AirAttackStep && airBulletSpawnPoint != null
            ? airBulletSpawnPoint
            : bulletSpawnPoint;
        // Spawn point là con của nhân vật (đã lật theo hướng mặt) => rotation khớp hướng sprite đạn;
        // hướng bay tính theo world, không phụ thuộc rotation
        Bullet bullet = ObjectPool.Spawn(prefab, muzzle.position, muzzle.rotation);
        bullet.Launch(Vector2.right * player.FacingDirection, attackInfo);
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
