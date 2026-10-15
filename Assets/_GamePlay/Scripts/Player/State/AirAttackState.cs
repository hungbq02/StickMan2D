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

        // Vũ khí bắn xa: đạn ra cùng frame với tư thế bắn (xem PlayerAnimationAttack.FireProjectile)
        WeaponDataSO weaponData = player.WeaponAttackManager.GetWeaponData(weapon);
        if (weaponData != null && weaponData.attackStyle == AttackStyle.Ranged)
            player.AnimationAttack.FireProjectile(WeaponAttackManager.AirAttackStep);
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
        // Fighter (đá chéo) / Sword giữ đòn tới khi chạm đất
        if (isFighterDive || isSwordAirAttack) return;

        // Bắn trên không: giữ tư thế bắn hết clip (đạn ra ở frame đầu) rồi mới rơi —
        // trước đây đổi sang Fall ngay khi vận tốc y < 0 nên tư thế bắn có thể chỉ hiện 1 frame
        if (player.AnimationController.IsDone())
        {
            ChangeState(States.Fall);
        }
    }
}
