using UnityEngine;

public class AttackState : State
{
    private IAttackHandler attackHandler;

    public override PlayerStateType StateType => PlayerStateType.attack;

    public AttackState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.ResetWallJumpLock();

        attackHandler = GetAttackHandler();
        attackHandler?.Attack();
    }

    public override void Update()
    {
        base.Update();

        attackHandler?.Update();

        if (attackHandler == null || attackHandler.IsDone)
        {
            ChangeState(States.Idle);
        }
    }

    public override void Exit()
    {
        base.Exit();
        // Bị ngắt giữa chừng (Hurt / Stun / đổi vũ khí) => handler không kẹt ở trạng thái đang đánh
        attackHandler?.Cancel();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.SetVelocityX(0f); // Dừng di chuyển khi tấn công
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
