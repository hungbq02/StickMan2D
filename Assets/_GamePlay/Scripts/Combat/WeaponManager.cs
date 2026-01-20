using Kore;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public WeaponType CurrentType { get; private set; } = WeaponType.Fighter;

    public void SetWeaponType(WeaponType type)
    {
        CurrentType = type;
        ObserverService.Notify(ObserverEnum.OnChangeWeapon);
    }
}
