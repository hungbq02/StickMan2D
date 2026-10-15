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

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.SetVelocityX(0f); // Dừng di chuyển khi tấn công
    }

    private IAttackHandler GetAttackHandler()
    {
        switch (player.WeaponManager.CurrentType)
        {
            case WeaponType.Fighter:
            case WeaponType.Sword:
                return player.MeleeHandler;
            case WeaponType.Gun:
                return player.GunHandler;
            default:
                Debug.LogWarning($"Unsupported weapon type: {player.WeaponManager.CurrentType}");
                return null;
        }
    }

    public void CancelAttack()
    {
        attackHandler?.Cancel();
        ChangeState(States.Idle);
    }
}
