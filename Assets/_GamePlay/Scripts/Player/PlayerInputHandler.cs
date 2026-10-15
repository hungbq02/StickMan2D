using UnityEngine;
using UnityEngine.InputSystem;

// Các nút bấm được giữ (buffer) trong inputBufferTime giây cho tới khi bị Consume,
// để state kế tiếp vẫn nhận được lệnh bấm hơi sớm (vd: bấm nhảy ngay trước khi chạm đất).
public class PlayerInputHandler : MonoBehaviour
{
    [Tooltip("Thời gian (giây) giữ lại 1 lần bấm nút cho tới khi được dùng")]
    [SerializeField] private float inputBufferTime = 0.1f;

    public Vector2 MoveInput { get; private set; }

    public bool JumpPressed => IsBuffered(jumpPressedTime);     //Key "Space"
    public bool AttackPressed => IsBuffered(attackPressedTime); //Key "Enter"
    public bool DashPressed => IsBuffered(dashPressedTime);     //Key "Shift"
    public bool SlidePressed => IsBuffered(slidePressedTime);   //Key "Ctrl"

    private float jumpPressedTime = float.NegativeInfinity;
    private float attackPressedTime = float.NegativeInfinity;
    private float dashPressedTime = float.NegativeInfinity;
    private float slidePressedTime = float.NegativeInfinity;

    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
            jumpPressedTime = Time.time;
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
            attackPressedTime = Time.time;
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
            dashPressedTime = Time.time;
    }

    public void OnSlide(InputValue value)
    {
        if (value.isPressed)
            slidePressedTime = Time.time;
    }

    public void ConsumeJump() => jumpPressedTime = float.NegativeInfinity;
    public void ConsumeAttack() => attackPressedTime = float.NegativeInfinity;
    public void ConsumeDash() => dashPressedTime = float.NegativeInfinity;
    public void ConsumeSlide() => slidePressedTime = float.NegativeInfinity;

    // Bỏ mọi nút đang buffer (vd: bấm trong lúc chết không được dùng ngay khi hồi sinh)
    public void ClearBuffer()
    {
        ConsumeJump();
        ConsumeAttack();
        ConsumeDash();
        ConsumeSlide();
    }

    private bool IsBuffered(float pressedTime) => Time.time - pressedTime <= inputBufferTime;
}
