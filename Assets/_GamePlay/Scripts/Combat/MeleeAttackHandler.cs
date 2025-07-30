using UnityEngine;

public class MeleeAttackHandler : IAttackHandler
{
    private readonly PlayerController player;
    private readonly AnimationController animationController;

    private float comboTimer;
    private float maxComboTime = 0.5f; // Thời gian cho phép để bấm tiếp combo
    private const int comboMaxStep = 3;

    private int comboStep = 0;
    private bool isAttacking = false;

    public bool IsDone => !isAttacking;

    public MeleeAttackHandler(PlayerController player, AnimationController anim)
    {
        this.player = player;
        this.animationController = anim;
    }

    public void Attack()
    {
        comboStep = 1;
        comboTimer = maxComboTime;
        isAttacking = true;

        UpdateCurrentAttackData();
        animationController.PlayAttack(player.weaponManager.CurrentType, comboStep);
    }

    public void Update()
    {
        if (!isAttacking) return;

        comboTimer -= Time.deltaTime;

        // Combo tiếp nếu nhấn tiếp tấn công và thời gian cho phép còn
        if (player.InputHandler.AttackPressed && comboTimer > 0 && comboStep < comboMaxStep)
        {
            comboStep++;
            comboTimer = maxComboTime;
            UpdateCurrentAttackData();
            animationController.PlayAttack(player.weaponManager.CurrentType, comboStep);
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
    }
    private void UpdateCurrentAttackData()
    {
        var currentAttackData = player.weaponAttackManager.GetCurrentAttackData(comboStep - 1); // comboStep từ 1 => index cần -1
        player.weaponAttackManager.SetCurrentAttackData(currentAttackData);
    }


}
