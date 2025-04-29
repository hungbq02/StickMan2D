using System;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public event Action OnWeaponChanged;
    public WeaponType CurrentType { get; private set; } = WeaponType.Fighter;

    public void SetWeaponType(WeaponType type)
    {
        CurrentType = type;
        OnWeaponChanged?.Invoke();
    }
}
