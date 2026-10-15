using UnityEngine;

// Prefab dùng chung cho runtime spawn (ObjectPool). Bootstrap đăng ký vào Service: Service.Get<PrefabRegistrySO>()
[CreateAssetMenu(fileName = "PrefabRegistry", menuName = "Data/Prefab Registry")]
public class PrefabRegistrySO : ScriptableObject
{
    public DamagePopup damagePopup;
    public GameObject ghostSprite;
    public Bullet bullet;
}
