using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponManager))]
public class WeaponAttackManager : MonoBehaviour
{
    [Tooltip("Chỉ dùng cho vũ khí cận chiến (fighter, sword). Gun được gắn ở Bullet")]
    [SerializeField] private List<WeaponAttackDataSO> weaponAttackDataList;

    private WeaponAttackDataSO currentWeaponAttackProfile;
    private AttackDataSO currentAttackData;
    public AttackDataSO CurrentAttackData => currentAttackData;

    private WeaponManager weaponManager;

    public void Initialize(WeaponManager weaponManager)
    {
        this.weaponManager = weaponManager;
        weaponManager.OnWeaponChanged += HandleWeaponChanged;

        currentWeaponAttackProfile = GetCurrentWeaponAttackProfile();
        currentAttackData = GetCurrentAttackData();
    }

    private void OnDestroy()
    {
        if (weaponManager != null)
            weaponManager.OnWeaponChanged -= HandleWeaponChanged;
    }

    public AttackDataSO GetCurrentAttackData(int comboIndex = 0)
    {
        if (currentWeaponAttackProfile != null &&
            comboIndex >= 0 &&
            comboIndex < currentWeaponAttackProfile.attackDataList.Count)
        {
            return currentWeaponAttackProfile.attackDataList[comboIndex];
        }
        return null;
    }

    private WeaponAttackDataSO GetCurrentWeaponAttackProfile()
    {
        return weaponAttackDataList.Find(p => p.weaponType == weaponManager.CurrentType);
    }

    public void SetCurrentAttackData(AttackDataSO data)
    {
        currentAttackData = data;
    }

    private void HandleWeaponChanged()
    {
        currentWeaponAttackProfile = GetCurrentWeaponAttackProfile();
        currentAttackData = GetCurrentAttackData();
    }
}
