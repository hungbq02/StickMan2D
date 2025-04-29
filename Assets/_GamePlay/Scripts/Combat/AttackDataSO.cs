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

    // Hàm tính xác suất chí mạng
    public bool IsCriticalHit()
    {
        return Random.Range(0f, 1f) <= criticalChance;
    }
}
