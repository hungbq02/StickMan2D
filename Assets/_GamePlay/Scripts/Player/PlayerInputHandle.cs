using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; } //Key "Space"
    public bool AttackPressed { get; private set; } //Key "Enter"
    public bool DashPressed { get; private set; } //Key "Shift"
    public bool SlidePressed { get; private set; } //Key "Ctrl"

    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
            JumpPressed = true;
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
            AttackPressed = true;
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
            DashPressed = true;
    }
    public void OnSlide(InputValue value)
    {
        if (value.isPressed)
            SlidePressed = true;
    }

    public void ResetInputs()
    {
        JumpPressed = false;
        AttackPressed = false;
        DashPressed = false;
        SlidePressed = false;
    }
}
