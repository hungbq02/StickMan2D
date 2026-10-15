using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponAttackData", menuName = "Data/WeaponAttackData")]
public class WeaponAttackDataSO : ScriptableObject
{
    public WeaponType weaponType;

    [Tooltip("Các đòn trên mặt đất theo thứ tự combo (Gun: 1 phần tử = bắn thường)")]
    public List<AttackDataSO> comboSteps = new List<AttackDataSO>();

    [Tooltip("Đòn trên không")]
    public AttackDataSO airAttack;
}
