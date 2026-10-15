using System.Collections.Generic;
using UnityEngine;

// Tra cứu AttackDataSO theo (vũ khí, bước đòn). Không giữ state "đòn hiện tại":
// animation event tự truyền bước đòn của clip nên đổi vũ khí / combo giữa chừng không đọc nhầm data.
public class WeaponAttackManager : MonoBehaviour
{
    // Quy ước tham số int của animation event
    public const int AirAttackStep = 0; // đòn trên không
    // 1..N = bước combo trên mặt đất (Gun: 1 = bắn thường)

    // Vị trí đòn trên không trong attackDataList (xem AttackComboIndex) — sẽ thay bằng field riêng ở CB-02
    private static readonly Dictionary<WeaponType, int> airAttackIndexMap = new Dictionary<WeaponType, int>
    {
        { WeaponType.Fighter, AttackComboIndex.Fighter.AirAttack },
        { WeaponType.Sword, AttackComboIndex.Sword.AirAttack },
        { WeaponType.Gun, AttackComboIndex.Gun.AirShoot },
    };

    [Tooltip("Chỉ dùng cho vũ khí cận chiến (fighter, sword). DataAttack Gun được gắn ở Bullet")]
    [SerializeField] private List<WeaponAttackDataSO> weaponAttackDataList;

    public AttackDataSO GetAttackData(WeaponType weaponType, int attackStep)
    {
        WeaponAttackDataSO profile = weaponAttackDataList.Find(p => p != null && p.weaponType == weaponType);
        if (profile == null) return null;

        bool hasAirAttack = airAttackIndexMap.TryGetValue(weaponType, out int airIndex);
        int index;
        if (attackStep == AirAttackStep)
        {
            if (!hasAirAttack) return null;
            index = airIndex;
        }
        else
        {
            index = attackStep - 1; // comboStep bắt đầu từ 1
            if (hasAirAttack && index == airIndex) return null; // bước combo không được trỏ vào đòn trên không
        }

        if (index < 0 || index >= profile.attackDataList.Count) return null;
        return profile.attackDataList[index];
    }
}
