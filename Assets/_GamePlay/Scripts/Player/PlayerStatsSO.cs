using UnityEngine;

// Chỉ số di chuyển / combat của player (asset: Data/PlayerStats). PlayerController.stats trỏ vào đây.
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Data/Player Stats")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 13f;
    [Tooltip("Thời gian (giây) sau khi rời mép vẫn được phép nhảy")]
    public float coyoteTime = 0.1f;
    [Tooltip("Tốc độ Kickdown (airAttack Fighter)")]
    public float diveSpeed = 25f;


    [Header("Wall Slide")]
    [Tooltip("Tốc độ trượt xuống khi nhân vật bám vào tường")]
    public float wallSlideSpeed = -1f;
    [Tooltip("Thời gian nhân vật bị khóa trong trạng thái nhảy tường trước khi chuyển sang trạng thái khác (giúp giữ quán tính nhảy).")]
    public float wallJumpDuration = 0.2f;
    [Tooltip("Lực nhảy theo phương ngang khi thực hiện nhảy khỏi tường.")]
    public float wallJumpForceX = 14f;
    [Tooltip("Lực nhảy theo phương thẳng đứng khi thực hiện nhảy khỏi tường.")]
    public float wallJumpForceY = 14f;

    [Header("Dash")]
    [Tooltip("Thời gian dash (giây)")]
    public float dashDuration = 0.15f;
    [Tooltip("Tốc độ dash")]
    public float dashSpeed = 20f;

    [Header("Slide")]
    [Tooltip("Thời gian trượt (giây)")]
    public float slideDuration = 0.2f;
    [Tooltip("Tốc độ trượt")]
    public float slideSpeed = 15f;
    [Tooltip("Tỉ lệ chiều cao collider khi trượt (0.5 = còn một nửa)")]
    public float slideHeightMultiplier = 0.5f;

    [Header("Climb")]
    [Tooltip("Tốc độ leo trèo")]
    public float climbSpeed = 5f;

    [Header("Combat")]
    [Tooltip("Thời gian cho phép bấm tiếp combo (fighter, sword)")]
    public float attackComboCooldown = 0.4f;

    [Header("Hurt")]
    [Tooltip("Thời gian mất điều khiển sau khi trúng đòn (giây)")]
    public float hurtDuration = 0.3f;
    [Tooltip("Thời gian bất tử (i-frame) sau khi trúng đòn (giây)")]
    public float invincibilityDuration = 0.8f;
    [Tooltip("Gia tốc hãm ngang (unit/s²) khi đứng trên đất mà không chạy (Idle, Land, Hurt, Stun) — triệt tiêu knockback. " +
             "Collider dùng NoFriction nên không hãm thì bị đẩy là trượt mãi")]
    public float groundDeceleration = 40f;

    [Header("Death")]
    [Tooltip("Thời gian (giây) từ lúc hết máu tới lúc hồi sinh tại điểm xuất phát")]
    public float respawnDelay = 2f;
}
