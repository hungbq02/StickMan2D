using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Tra cứu WeaponDataSO / AttackDataSO theo (vũ khí, bước đòn). Không giữ state "đòn hiện tại":
// animation event tự truyền bước đòn của clip nên đổi vũ khí / combo giữa chừng không đọc nhầm data.
public class WeaponAttackManager : MonoBehaviour
{
    // Quy ước tham số int của animation event
    public const int AirAttackStep = 0; // đòn trên không
    // 1..N = bước combo trên mặt đất (Ranged: 1 = bắn thường)

    [FormerlySerializedAs("weaponAttackDataList")]
    [SerializeField] private List<WeaponDataSO> weapons;

    public WeaponDataSO GetWeaponData(WeaponType weaponType)
    {
        return weapons.Find(p => p != null && p.weaponType == weaponType);
    }

    public AttackDataSO GetAttackData(WeaponType weaponType, int attackStep)
    {
        WeaponDataSO weapon = GetWeaponData(weaponType);
        if (weapon == null) return null;

        if (attackStep == AirAttackStep) return weapon.airAttack;

        int index = attackStep - 1; // comboStep bắt đầu từ 1
        if (index < 0 || index >= weapon.comboSteps.Count) return null;
        return weapon.comboSteps[index];
    }

    public int GetComboStepCount(WeaponType weaponType)
    {
        WeaponDataSO weapon = GetWeaponData(weaponType);
        return weapon != null ? weapon.comboSteps.Count : 0;
    }
}
