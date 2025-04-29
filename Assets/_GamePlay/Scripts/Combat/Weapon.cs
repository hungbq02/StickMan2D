using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public WeaponType weaponType;
    public abstract void Attack(PlayerController player);
}
