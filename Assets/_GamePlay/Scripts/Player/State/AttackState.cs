using UnityEngine;

// Đánh trên mặt đất. Mỗi bước đòn: quay theo hướng đang giữ, bước tới (AttackDataSO.lunge*, giữ hướng => xa hơn),
// tốc độ trong lúc đánh ≤ attackMoveSpeedRatio × moveSpeed rồi hãm dần (AttackMotion). Qua frame trúng: Jump / Dash / Slide ngắt được phần thu đòn,
// giữ hướng di chuyển thêm stats.attackMoveCancelDelay thì chạy luôn. Rời đòn chủ động / hết clip => giữ bước combo
// (bấm lại trong cửa sổ combo đánh tiếp bước sau); bị đánh / đổi vũ khí => Cancel, combo về 1.
public class AttackState : State
{
    private const float MoveDeadZone = 0.01f;

    private IAttackHandler attackHandler;
    private AttackMotion motion;
    private int lastStepSerial;
    private bool released;

    public override PlayerStateType StateType => PlayerStateType.attack;

    public AttackState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.ResetWallJumpLock();
        released = false;
        motion.Stop();

        attackHandler = GetAttackHandler();
        if (attackHandler == null) return;
        lastStepSerial = attackHandler.StepSerial;
        attackHandler.Attack();
        BeginStepIfNew();
    }

    public override void Update()
    {
        base.Update();

        if (attackHandler == null)
        {
            ChangeToDefaultState();
            return;
        }

        attackHandler.Update();

        if (attackHandler.IsDone)
        {
            Leave(MoveHeld() && player.GroundCheck.IsGrounded ? States.Run : DefaultState());
            return;
        }

        BeginStepIfNew();
        TryCancel();
    }

    public override void Exit()
    {
        base.Exit();
        // Bị ngắt giữa chừng (Hurt / Stun / đổi vũ khí / chết) => handler không kẹt ở trạng thái đang đánh, combo về 1
        if (released) attackHandler?.Release();
        else attackHandler?.Cancel();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        int facing = player.FacingDirection;
        var stats = player.stats;
        float vx = motion.Step(player.Rigidbody.velocity.x, facing, Time.fixedDeltaTime, stats.attackDeceleration,
            stats.moveSpeed * stats.attackMoveSpeedRatio);
        // Không lao / trượt ra khỏi mép vực
        if (vx * facing > 0f && !player.GroundCheck.HasGroundAhead(facing))
        {
            motion.Stop();
            vx = 0f;
        }
        player.SetVelocityX(vx);
    }

    // Bước đòn mới bắt đầu (đòn đầu hoặc combo kế): quay theo hướng giữ rồi lao tới
    private void BeginStepIfNew()
    {
        if (attackHandler.StepSerial == lastStepSerial) return;
        lastStepSerial = attackHandler.StepSerial;

        float moveX = player.InputHandler.MoveInput.x;
        bool held = Mathf.Abs(moveX) > MoveDeadZone;
        if (held) player.CheckIfShouldFlip(moveX);

        AttackDataSO data = player.WeaponAttackManager.GetAttackData(player.WeaponManager.CurrentType, attackHandler.CurrentStep);
        if (data == null)
        {
            motion.Stop();
            return;
        }
        float distance = data.lungeDistance * (held ? player.stats.attackLungeHoldMultiplier : 1f);
        motion.Begin(distance, data.lungeDuration);
    }

    // Thứ tự = độ ưu tiên
    private void TryCancel()
    {
        var input = player.InputHandler;
        var groundCheck = player.GroundCheck;

        if (attackHandler.CanActionCancel)
        {
            if (input.JumpPressed && player.CanGroundJump)
            {
                input.ConsumeJump();
                Leave(States.Jump);
                return;
            }
            if (input.DashPressed && groundCheck.IsGrounded)
            {
                input.ConsumeDash();
                Leave(States.Dash);
                return;
            }
            if (input.SlidePressed && groundCheck.IsGrounded)
            {
                input.ConsumeSlide();
                Leave(States.Slide);
                return;
            }
        }

        // Còn nút đánh đang chờ => ưu tiên đánh tiếp, không chạy
        if (attackHandler.CanMoveCancel && !input.AttackPressed && MoveHeld() && groundCheck.IsGrounded)
            Leave(States.Run);
    }

    private bool MoveHeld() => Mathf.Abs(player.InputHandler.MoveInput.x) > MoveDeadZone;

    private State DefaultState() => player.GroundCheck.IsGrounded ? States.Idle : (State)States.Fall;

    // Rời đòn chủ động: giữ bước combo
    private void Leave(State next)
    {
        released = true;
        ChangeState(next);
    }

    // Chọn handler theo kiểu đánh khai báo trong WeaponDataSO (không hardcode theo WeaponType)
    private IAttackHandler GetAttackHandler()
    {
        WeaponType weaponType = player.WeaponManager.CurrentType;
        WeaponDataSO weapon = player.WeaponAttackManager.GetWeaponData(weaponType);
        if (weapon == null)
        {
            Debug.LogWarning($"[AttackState] Không có WeaponDataSO cho {weaponType}");
            return null;
        }
        return weapon.attackStyle == AttackStyle.Ranged ? player.GunHandler : player.MeleeHandler;
    }

    public void CancelAttack()
    {
        attackHandler?.Cancel();
        ChangeState(States.Idle);
    }
}
