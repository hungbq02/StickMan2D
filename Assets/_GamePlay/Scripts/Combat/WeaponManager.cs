using System;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public WeaponType CurrentType { get; private set; } = WeaponType.Fighter;

    // Event cục bộ của nhân vật này (không dùng ObserverService global)
    public event Action<WeaponType> OnWeaponChanged;

    public void SetWeaponType(WeaponType type)
    {
        if (type == CurrentType) return;

        CurrentType = type;
        OnWeaponChanged?.Invoke(type);
    }

    // Nút đổi vũ khí (cảm ứng / Q / RB): xoay vòng Fighter -> Sword -> Gun -> Fighter
    public void CycleWeapon()
    {
        var types = (WeaponType[])Enum.GetValues(typeof(WeaponType));
        int i = Array.IndexOf(types, CurrentType);
        SetWeaponType(types[(i + 1) % types.Length]);
    }
}
