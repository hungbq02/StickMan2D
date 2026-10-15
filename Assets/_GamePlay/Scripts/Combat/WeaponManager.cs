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
}
