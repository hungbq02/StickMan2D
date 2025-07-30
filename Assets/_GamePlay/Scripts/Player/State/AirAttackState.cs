using System.Collections.Generic;
using UnityEngine;

public class AirAttackState : State
{
    private IAttackHandler attackHandler;
    private bool isFighterDive;
    private bool isSwordAirAttack;
    private Vector2 diveDirection;

    //Key: WeaponType, Value: AttackComboIndex
    // This dictionary maps the weapon type to the corresponding attack index for air attacks.
    private static readonly Dictionary<WeaponType, int> airAttackIndexMap = new Dictionary<WeaponType, int>
{
    { WeaponType.Fighter, AttackComboIndex.Fighter.AirAttack },
    { WeaponType.Sword, AttackComboIndex.Sword.AirAttack },
    { WeaponType.Gun, AttackComboIndex.Gun.AirShoot },
};

    public AirAttackState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateType stateType)
        : base(stateMachine, player, stateType)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (airAttackIndexMap.TryGetValue(player.weaponManager.CurrentType, out int attackIndex))
        {
            Debug.Log($"AirAttack: {player.weaponManager.CurrentType} - {attackIndex}");
            //    player.SetCurrentAttackData(player.GetCurrentAttackData(attackIndex)); //Set AttackData cho AirAttack
            player.weaponAttackManager.SetCurrentAttackData(player.weaponAttackManager.GetCurrentAttackData(attackIndex)); //Set AttackData cho AirAttack
            //AirAttack Fighter
            isFighterDive = player.weaponManager.CurrentType == WeaponType.Fighter;
            isSwordAirAttack = player.weaponManager.CurrentType == WeaponType.Sword;

            if (isFighterDive)
            {
                // Đá chéo: hướng 45 độ xuống theo hướng nhân vật
                diveDirection = new Vector2(player.FacingDirection, -0.2f).normalized;
                player.Rigidbody.velocity = diveDirection * player.stats.diveSpeed;
            }
        }
        else
        {
            Debug.LogWarning($"WeaponType {player.weaponManager.CurrentType} không có AirAttack mapping.");
        }

        player.AnimationController.PlayAirAttack(player.weaponManager.CurrentType); //Animation AirAttack
    }

    public override void Update()
    {
        base.Update();


        if (player.GroundCheck.IsGrounded)
        {
            // Nếu chạm đất thì chuyển về trạng thái Land
            player.SetVelocityX(0f);
            stateMachine.ChangeState(new LandState(stateMachine, player, PlayerStateType.land));
        }
        //Fall
        if (player.Rigidbody.velocity.y < 0 && !isFighterDive && !isSwordAirAttack)
        {
            stateMachine.ChangeState(new FallState(stateMachine, player, PlayerStateType.fall));
        }
        //  player.AnimationController.PlayAirAttack(player.weaponManager.CurrentType); //Animation AirAttack
    }


    public override void Exit()
    {
        base.Exit();
    }
}
