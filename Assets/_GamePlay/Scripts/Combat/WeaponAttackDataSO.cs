using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponAttackData", menuName = "Data/WeaponAttackData")]
public class WeaponAttackDataSO : ScriptableObject
{
    public WeaponType weaponType;
    public List<AttackDataSO> attackDataList = new List<AttackDataSO>();
}
