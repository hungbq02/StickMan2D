using UnityEngine;

public class MeleeAttackHandler : IAttackHandler
{
    private const int ComboMaxStep = 3;

    private readonly PlayerController player;
    private readonly AnimationController animationController;

    private float comboTimer;
    private int comboStep = 0;
    private bool isAttacking = false;

    public bool IsDone => !isAttacking;

    // Thời gian cho phép để bấm tiếp combo
    private float ComboWindow => player.stats.attackComboCooldown;

    public MeleeAttackHandler(PlayerController player, AnimationController anim)
    {
        this.player = player;
        this.animationController = anim;
    }

    public void Attack()
    {
        comboStep = 1;
        comboTimer = ComboWindow;
        isAttacking = true;

        animationController.PlayAttack(player.WeaponManager.CurrentType, comboStep);
    }

    public void Update()
    {
        if (!isAttacking) return;

        comboTimer -= Time.deltaTime;

        // Combo tiếp nếu nhấn tiếp tấn công và thời gian cho phép còn
        var input = player.InputHandler;
        if (input.AttackPressed && comboTimer > 0 && comboStep < ComboMaxStep)
        {
            input.ConsumeAttack();
            comboStep++;
            comboTimer = ComboWindow;
            animationController.PlayAttack(player.WeaponManager.CurrentType, comboStep);
        }

        // when animation done but don't press attack => reset combo
        if (animationController.IsDone())
        {
            if (comboStep >= ComboMaxStep || comboTimer <= 0)
            {
                isAttacking = false;
            }
        }
    }

    public void Cancel()
    {
        isAttacking = false;
        comboStep = 0;
        comboTimer = 0f;
    }
}
