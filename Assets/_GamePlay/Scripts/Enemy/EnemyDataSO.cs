using UnityEngine;

// Chỉ số + hành vi của 1 loại enemy (EnemyController đọc). Mỗi loại enemy = 1 asset.
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Data/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Stats")]
    public int maxHealth = 50;

    [Header("Movement")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3f;
    [Tooltip("Đi tuần qua lại quanh vị trí xuất phát ± khoảng này")]
    public float patrolDistance = 3f;
    [Tooltip("Thời gian đứng nghỉ ở mỗi đầu đường tuần tra")]
    public float patrolPause = 0.6f;

    [Header("Detection")]
    [Tooltip("Thấy player trong khoảng này (theo trục X) => đuổi")]
    public float detectRange = 6f;
    [Tooltip("Player ra ngoài khoảng này => bỏ đuổi, quay về tuần tra")]
    public float loseRange = 9f;
    [Tooltip("Chênh lệch độ cao tối đa để còn coi là thấy / đánh được")]
    public float maxHeightDifference = 1.5f;

    [Header("Attack")]
    public AttackDataSO attack;
    public float attackRange = 1.2f;
    [Tooltip("Thời gian vung đòn trước khi gây damage (player có thể né)")]
    public float attackWindup = 0.45f;
    [Tooltip("Thời gian đứng yên sau khi đánh")]
    public float attackRecovery = 0.35f;
    [Tooltip("Thời gian chờ tối thiểu giữa 2 đòn")]
    public float attackCooldown = 1.2f;

    [Header("Hurt")]
    [Tooltip("Thời gian khựng lại khi bị đánh")]
    public float hurtDuration = 0.25f;
}
