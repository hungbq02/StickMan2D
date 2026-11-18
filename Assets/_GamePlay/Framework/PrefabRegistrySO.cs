using UnityEngine;

// Prefab dùng chung cho runtime spawn (ObjectPool). Bootstrap đăng ký vào Service: Service.Get<PrefabRegistrySO>()
[CreateAssetMenu(fileName = "PrefabRegistry", menuName = "Data/Prefab Registry")]
public class PrefabRegistrySO : ScriptableObject
{
    public DamagePopup damagePopup;
    public GameObject ghostSprite;
    public Bullet bullet;
    [Header("Hiệu ứng trúng đòn (HitFeedback)")]
    public HitFx hitFx;
    public HitFx critFx;
    public HitFx blockFx;
    [Header("Hiệu ứng khác")]
    [Tooltip("Xác enemy tan thành khói (EnemyDeadState, lúc bắt đầu mờ)")]
    public HitFx enemyDeathFx;
}
