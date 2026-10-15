using System.Collections.Generic;
using UnityEngine;

public enum AttackStyle
{
    Melee,  // combo nhiều đòn, damage qua Hitbox (MeleeAttackHandler)
    Ranged, // bắn projectile (GunAttackHandler)
}

// Data 1 vũ khí: kiểu đánh, các đòn (combo + trên không), projectile. Thêm vũ khí mới = thêm asset + clip animation.
[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Data/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    public WeaponType weaponType;
    public AttackStyle attackStyle = AttackStyle.Melee;

    [Tooltip("Các đòn trên mặt đất theo thứ tự combo (Ranged: 1 phần tử = bắn thường)")]
    public List<AttackDataSO> comboSteps = new List<AttackDataSO>();

    [Tooltip("Đòn trên không")]
    public AttackDataSO airAttack;

    [Tooltip("Chỉ dùng cho Ranged — để trống => dùng bullet trong PrefabRegistry")]
    public Bullet projectile;
}
