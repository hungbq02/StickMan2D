using UnityEngine;

// 1 đòn theo EnemyAttackSO: Telegraph (tư thế báo hiệu + tô màu) -> Active (animation đòn, các mốc trúng,
// lao / lùi / dịch chuyển) -> Recovery (đứng yên) -> Chase. Bị choáng / đóng băng giữa chừng => Hurt.
public class EnemyAttackState : EnemyState
{
    private enum Stage { Telegraph, Active, Recovery }

    private EnemyAttackSO attack;
    private Stage stage;
    private float timer;
    private float activeTime;
    private float activeDuration;
    private int nextHit;
    private bool teleported;

    public EnemyAttackSO Current => attack;

    public EnemyAttackState(EnemyController enemy) : base(enemy) { }

    public void Setup(EnemyAttackSO attackSO)
    {
        attack = attackSO;
    }

    public override void Enter()
    {
        stage = Stage.Telegraph;
        nextHit = 0;
        teleported = false;
        timer = attack.telegraphTime * enemy.TempoMultiplier;
        enemy.SetVelocityX(0f);
        if (enemy.HasTarget)
            enemy.FaceTowards(enemy.Target.position.x);
        if (attack.tintTelegraph)
            enemy.SetWindupTint(true);
        if (string.IsNullOrEmpty(attack.telegraphAnim) || enemy.Anim == null || !enemy.Anim.Has(attack.telegraphAnim))
            enemy.PlayAnim(enemy.IdleAnim);
        else
            enemy.PlayAnim(attack.telegraphAnim, restart: true);

        if (timer <= 0f)
            BeginActive();
    }

    public override void Update()
    {
        if (!enemy.CanAct)
        {
            ChangeState(enemy.Hurt);
            return;
        }

        switch (stage)
        {
            case Stage.Telegraph:
                timer -= Time.deltaTime;
                if (timer <= 0f) BeginActive();
                break;

            case Stage.Active:
                activeTime += Time.deltaTime;
                if (attack.movement == EnemyAttackMovement.Teleport && !teleported && activeTime >= attack.moveStart)
                    TeleportBehindTarget();
                while (attack.hitTimes != null && nextHit < attack.hitTimes.Length && activeTime >= attack.hitTimes[nextHit])
                    enemy.ExecuteHit(attack, nextHit++);
                if (activeTime >= activeDuration)
                    BeginRecovery();
                break;

            case Stage.Recovery:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                    ChangeState(enemy.HasTarget ? (EnemyState)enemy.Chase : enemy.Patrol);
                break;
        }
    }

    public override void FixedUpdate()
    {
        bool moving = stage == Stage.Active && activeTime >= attack.moveStart && activeTime <= attack.moveEnd;
        if (moving && attack.movement == EnemyAttackMovement.Lunge)
            enemy.MoveForward(attack.moveSpeed * enemy.SpeedMultiplier);
        else if (moving && attack.movement == EnemyAttackMovement.Retreat)
            enemy.MoveTowardsDirection(-enemy.FacingDirection, attack.moveSpeed * enemy.SpeedMultiplier);
        else
            enemy.SetVelocityX(0f);
    }

    public override void Exit()
    {
        enemy.SetWindupTint(false);
        // Bị ngắt lúc đang báo hiệu (Hurt / phase) => vẫn tính cooldown để không ra lại ngay
        if (stage == Stage.Telegraph)
            enemy.MarkAttackUsed(attack);
    }

    private void BeginActive()
    {
        stage = Stage.Active;
        activeTime = 0f;
        enemy.SetWindupTint(false);
        enemy.MarkAttackUsed(attack);
        enemy.PlayAnim(attack.attackAnim, restart: true);
        activeDuration = attack.ResolveActiveDuration(enemy.AnimLength(attack.attackAnim));
    }

    private void BeginRecovery()
    {
        stage = Stage.Recovery;
        timer = attack.recoveryTime;
        enemy.SetVelocityX(0f);
        enemy.PlayAnim(enemy.IdleAnim);
    }

    private void TeleportBehindTarget()
    {
        teleported = true;
        if (!enemy.HasTarget) return;
        float targetX = enemy.Target.position.x;
        float side = Mathf.Sign(targetX - enemy.transform.position.x);
        if (side == 0f) side = enemy.FacingDirection;
        enemy.TeleportToX(targetX + side * attack.moveSpeed);
        enemy.FaceTowards(targetX);
    }
}
