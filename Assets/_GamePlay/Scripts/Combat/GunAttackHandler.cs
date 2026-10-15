public class GunAttackHandler : IAttackHandler
{
    private readonly PlayerController player;
    private readonly AnimationController animationController;

    private bool isShooting = false;
    public bool IsDone => !isShooting;

    public GunAttackHandler(PlayerController player, AnimationController anim)
    {
        this.player = player;
        this.animationController = anim;
    }

    public void Attack()
    {
        isShooting = true;
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

    public void Cancel()
    {
        isShooting = false;
    }
}
