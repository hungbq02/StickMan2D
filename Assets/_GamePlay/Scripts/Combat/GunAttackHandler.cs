using UnityEngine;

public class GunAttackHandler : IAttackHandler
{
    private readonly PlayerController player;
    private readonly AnimationController animationController;

    private bool isShooting = false;
    private float shotTime;

    public bool IsDone => !isShooting;
    public int CurrentStep => 1;
    public int StepSerial { get; private set; }
    // Đạn ra ngay frame đầu => nhảy / dash / trượt ngắt được luôn
    public bool CanActionCancel => isShooting;
    public bool CanMoveCancel => isShooting && Time.time - shotTime >= player.stats.gunMoveCancelDelay;

    public GunAttackHandler(PlayerController player, AnimationController anim)
    {
        this.player = player;
        this.animationController = anim;
    }

    public void Attack()
    {
        isShooting = true;
        shotTime = Time.time;
        StepSerial++;
        // Phát lại clip từ đầu (frame giật súng) và bắn đạn trong cùng frame => hình và đạn luôn khớp
        animationController.PlayAttack(player.WeaponManager.CurrentType);
        player.AnimationAttack.FireProjectile(1);
    }

    public void Update()
    {
        if (isShooting && animationController.IsDone())
        {
            isShooting = false;
        }
    }

    public void OnHitFrame() { }

    public void Release()
    {
        isShooting = false;
    }

    public void Cancel()
    {
        isShooting = false;
    }
}
