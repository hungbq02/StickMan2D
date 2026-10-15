using System.Collections.Generic;
using UnityEngine;

// Tra cứu AttackDataSO theo (vũ khí, bước đòn). Không giữ state "đòn hiện tại":
// animation event tự truyền bước đòn của clip nên đổi vũ khí / combo giữa chừng không đọc nhầm data.
public class WeaponAttackManager : MonoBehaviour
{
    // Quy ước tham số int của animation event
    public const int AirAttackStep = 0; // đòn trên không
    // 1..N = bước combo trên mặt đất (Gun: 1 = bắn thường)

    [SerializeField] private List<WeaponAttackDataSO> weaponAttackDataList;

    public AttackDataSO GetAttackData(WeaponType weaponType, int attackStep)
    {
        WeaponAttackDataSO profile = GetProfile(weaponType);
        if (profile == null) return null;

        if (attackStep == AirAttackStep) return profile.airAttack;

        int index = attackStep - 1; // comboStep bắt đầu từ 1
        if (index < 0 || index >= profile.comboSteps.Count) return null;
        return profile.comboSteps[index];
    }

    public int GetComboStepCount(WeaponType weaponType)
    {
        WeaponAttackDataSO profile = GetProfile(weaponType);
        return profile != null ? profile.comboSteps.Count : 0;
    }

    private WeaponAttackDataSO GetProfile(WeaponType weaponType)
    {
        return weaponAttackDataList.Find(p => p != null && p.weaponType == weaponType);
    }
}
