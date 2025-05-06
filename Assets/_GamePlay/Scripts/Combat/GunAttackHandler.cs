using UnityEngine;

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
        //The gun has no combo, the default attackData is 0
        player.SetCurrentAttackData(player.GetCurrentAttackData(0));

        animationController.PlayAttack(player.weaponManager.CurrentType);
    }

    public void Update()
    {
        if (!isShooting) return;

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
