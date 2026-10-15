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
        animationController.PlayAttack(player.WeaponManager.CurrentType);
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
