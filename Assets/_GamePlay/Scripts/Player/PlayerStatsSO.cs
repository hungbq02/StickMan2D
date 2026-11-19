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
    [Tooltip("Đã bấm đòn kế: ra đòn kế sau frame trúng của đòn hiện tại bao lâu (giây) — đủ thấy tư thế trúng")]
    public float comboChainDelay = 0.06f;
    [Tooltip("Giữ hướng tiến khi ra đòn => quãng bước (AttackDataSO.lungeDistance) nhân hệ số này")]
    public float attackLungeHoldMultiplier = 1.25f;
    [Tooltip("Trần tốc độ ngang trong lúc đánh = tỉ lệ này × moveSpeed (đang chạy vào đòn => tụt ngay về trần)")]
    [Range(0f, 1f)] public float attackMoveSpeedRatio = 0.35f;
    [Tooltip("Gia tốc hãm ngang (unit/s²) sau khi hết bước tới")]
    public float attackDeceleration = 30f;
    [Tooltip("Sau frame trúng đòn bao lâu (giây) thì giữ hướng di chuyển được ngắt phần thu đòn để chạy")]
    public float attackMoveCancelDelay = 0.08f;
    [Tooltip("Súng: sau phát bắn bao lâu (giây) thì di chuyển được ngắt tư thế bắn")]
    public float gunMoveCancelDelay = 0.1f;

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
