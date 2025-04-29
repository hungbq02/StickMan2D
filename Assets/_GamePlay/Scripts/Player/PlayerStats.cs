using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    [Tooltip("Tốc độ Kickdown (airAttack Fighter)")]
    public float diveSpeed = 15f;


    [Header("Wall Slide")]
    [Tooltip("Tốc độ trượt xuống khi nhân vật bám vào tường")]
    public float wallSlideSpeed = -1f;
    [Tooltip("Thời gian nhân vật bị khóa trong trạng thái nhảy tường trước khi chuyển sang trạng thái khác (giúp giữ quán tính nhảy).")]
    public float wallJumpDuration = 0.25f;
    [Tooltip("Lực nhảy theo phương ngang khi thực hiện nhảy khỏi tường.")]
    public float wallJumpForceX = 6f;
    [Tooltip("Lực nhảy theo phương thẳng đứng khi thực hiện nhảy khỏi tường.")]
    public float wallJumpForceY = 12f;

    [Header("Dash")]
    public float dashDuration = 0.2f; 
    public float dashSpeed = 20f;     


    [Header("Combat")]
    [Tooltip("Thời gian cho phép giữ trạng thái combo (fighter, sword)")]
    public float attackComboCooldown = 0.4f;
    [Tooltip("sát thương chí mạng (gốc * criticalDamageMultiplier)")]
    public float criticalDamageMultiplier = 2f;

    [Header("Physics")]
    public float gravityScale = 1f;
}
