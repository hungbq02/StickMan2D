using UnityEngine;

public class AttackState : State
{
    private IAttackHandler attackHandler;
    public AttackState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateType stateType)
        : base(stateMachine, player, stateType)
    {
    }

    public override void Enter()
    {
        player.ResetWallJumpLock();
        base.Enter();

        attackHandler = GetAttackHandler();
        attackHandler?.Attack();

    }

    public override void Update()
    {
        base.Update();

        attackHandler.Update();

        if (attackHandler.IsDone)
        {
            stateMachine.ChangeState(new IdleState(stateMachine, player, PlayerStateType.idle));
        }
        //Jump
        if (player.InputHandler.JumpPressed)
        {
            stateMachine.ChangeState(new JumpState(stateMachine, player, PlayerStateType.jump));
            return;
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.Rigidbody.velocity = new Vector2(0f, player.Rigidbody.velocity.y); // Dừng di chuyển khi tấn công

    }
    //
    private IAttackHandler GetAttackHandler()
    {
        switch (player.weaponManager.CurrentType)
        {
            case WeaponType.Fighter:
            case WeaponType.Sword:
                return player.meleeHandler;
            case WeaponType.Gun:
                return player.gunHandler;
            default:
                Debug.LogWarning("Unsupported weapon type!");
                return null;
        }
    }



}
