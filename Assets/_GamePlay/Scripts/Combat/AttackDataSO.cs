using UnityEngine;

[CreateAssetMenu(fileName = "NewAttackData", menuName = "Attack Data", order = 0)]
public class AttackDataSO : ScriptableObject
{
    public int damage; // Sát thương
    public float knockbackForce; // Lực đẩy của đòn tấn công
    public float criticalChance; // Xác suất chí mạng
    public float criticalMultiplier; // Hệ số chí mạng
    public float range; // Khoảng cách tấn công
    public float effectDuration; // Thời gian hiệu ứng (nếu có)
    public AttackEffect attackEffect; // Loại hiệu ứng

    [Tooltip("Hiệu ứng trúng đòn riêng của đòn này (trống = PrefabRegistrySO.hitFx)")]
    public HitFx hitFx;
    [Tooltip("Cỡ hiệu ứng trúng đòn (đòn kết combo to hơn)")]
    public float hitFxScale = 1f;
    [Tooltip("Rung màn hình khi trúng (trauma 0..1, CameraShake) — 0 = dùng mức tối thiểu của bên bị đánh")]
    [Range(0f, 1f)] public float shake;
}
