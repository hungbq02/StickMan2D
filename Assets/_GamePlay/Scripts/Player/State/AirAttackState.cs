using UnityEngine;

public class AirAttackState : State
{
    private bool isFighterDive;
    private bool isSwordAirAttack;

    public override PlayerStateType StateType => PlayerStateType.airAttack;

    public AirAttackState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();

        WeaponType weapon = player.WeaponManager.CurrentType;
        isFighterDive = weapon == WeaponType.Fighter;
        isSwordAirAttack = weapon == WeaponType.Sword;

        if (isFighterDive)
        {
            // Đá chéo xuống theo hướng nhân vật
            Vector2 diveDirection = new Vector2(player.FacingDirection, -0.2f).normalized;
            player.Rigidbody.velocity = diveDirection * player.stats.diveSpeed;
        }

        player.AnimationController.PlayAirAttack(weapon);
    }

    public override void Update()
    {
        base.Update();

        if (player.GroundCheck.IsGrounded)
        {
            // Chạm đất thì chuyển về trạng thái Land
            player.SetVelocityX(0f);
            ChangeState(States.Land);
            return;
        }
        //Fall
        if (player.Rigidbody.velocity.y < 0 && !isFighterDive && !isSwordAirAttack)
        {
            ChangeState(States.Fall);
        }
    }
}
