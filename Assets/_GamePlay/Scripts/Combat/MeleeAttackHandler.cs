using UnityEngine;

public class MeleeAttackHandler : IAttackHandler
{
    private readonly PlayerController player;
    private readonly AnimationController animationController;

    private float comboTimer;
    private int comboStep = 0;
    private int comboMaxStep = 0; // số đòn combo của vũ khí lúc bắt đầu đánh (WeaponDataSO.comboSteps)
    private bool isAttacking = false;
    // Bấm đòn kế trước frame trúng của đòn hiện tại => xếp hàng, ra đòn kế ngay khi đòn này trúng
    private bool comboQueued;
    private bool hitDone;
    private float hitTime;

    // Bước gần nhất (giữ lại khi rời đòn chủ động) — bấm lại trong cửa sổ combo thì đánh tiếp bước sau
    private int lastStep;
    private float lastStepStartTime = float.NegativeInfinity;
    private WeaponType lastWeapon;

    public bool IsDone => !isAttacking;
    public int CurrentStep => comboStep;
    public int StepSerial { get; private set; }
    public bool CanActionCancel => isAttacking && hitDone;
    public bool CanMoveCancel => isAttacking && hitDone && !comboQueued
                                 && Time.time - hitTime >= player.stats.attackMoveCancelDelay;

    // Thời gian cho phép để bấm tiếp combo (tính từ lúc bắt đầu bước đòn)
    private float ComboWindow => player.stats.attackComboCooldown;

    public MeleeAttackHandler(PlayerController player, AnimationController anim)
    {
        this.player = player;
        this.animationController = anim;
    }

    // Bước combo khi bắt đầu đánh: tiếp bước sau nếu vừa rời đòn trước còn trong cửa sổ combo, không thì về 1
    public static int NextComboStep(int lastStep, float sinceLastStepStart, float comboWindow, int maxStep)
    {
        if (lastStep > 0 && lastStep < maxStep && sinceLastStepStart <= comboWindow)
            return lastStep + 1;
        return 1;
    }

    public void Attack()
    {
        WeaponType weapon = player.WeaponManager.CurrentType;
        comboMaxStep = player.WeaponAttackManager.GetComboStepCount(weapon);
        int resumeFrom = weapon == lastWeapon ? lastStep : 0;
        comboStep = NextComboStep(resumeFrom, Time.time - lastStepStartTime, ComboWindow, comboMaxStep);
        isAttacking = true;
        StartStep(weapon);
    }

    private void StartStep(WeaponType weapon)
    {
        comboTimer = ComboWindow;
        comboQueued = false;
        hitDone = false;
        lastStep = comboStep;
        lastStepStartTime = Time.time;
        lastWeapon = weapon;
        StepSerial++;
        animationController.PlayAttack(weapon, comboStep);
    }

    public void Update()
    {
        if (!isAttacking) return;

        comboTimer -= Time.deltaTime;

        // Bấm tiếp trong cửa sổ combo => xếp hàng đòn kế
        var input = player.InputHandler;
        if (input.AttackPressed && comboTimer > 0 && comboStep < comboMaxStep)
        {
            input.ConsumeAttack();
            comboQueued = true;
        }

        bool clipDone = animationController.IsDone();

        // Ra đòn kế khi đòn hiện tại đã trúng (không cắt ngang trước frame trúng); clip thiếu event => chờ hết clip
        if (comboQueued && ((hitDone && Time.time - hitTime >= player.stats.comboChainDelay) || clipDone))
        {
            comboStep++;
            StartStep(player.WeaponManager.CurrentType);
            return;
        }

        // Hết clip => rời đòn ngay (không đứng chờ hết cửa sổ combo); bấm lại kịp thì Attack() đánh tiếp bước sau
        if (clipDone)
            isAttacking = false;
    }

    public void OnHitFrame()
    {
        if (!isAttacking || hitDone) return;
        hitDone = true;
        hitTime = Time.time;
    }

    public void Release()
    {
        isAttacking = false;
        comboQueued = false;
    }

    public void Cancel()
    {
        isAttacking = false;
        comboQueued = false;
        comboStep = 0;
        comboTimer = 0f;
        lastStep = 0;
    }
}
