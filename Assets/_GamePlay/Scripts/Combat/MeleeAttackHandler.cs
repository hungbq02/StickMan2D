using UnityEngine;

public class MeleeAttackHandler : IAttackHandler
{
    private readonly PlayerController player;
    private readonly AnimationController animationController;

    private float comboTimer;
    private int comboStep = 0;
    private int comboMaxStep = 0; // số đòn combo của vũ khí lúc bắt đầu đánh (WeaponDataSO.comboSteps)
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
        comboMaxStep = player.WeaponAttackManager.GetComboStepCount(player.WeaponManager.CurrentType);
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
        if (input.AttackPressed && comboTimer > 0 && comboStep < comboMaxStep)
        {
            input.ConsumeAttack();
            comboStep++;
            comboTimer = ComboWindow;
            animationController.PlayAttack(player.WeaponManager.CurrentType, comboStep);
        }

        // when animation done but don't press attack => reset combo
        if (animationController.IsDone())
        {
            if (comboStep >= comboMaxStep || comboTimer <= 0)
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
