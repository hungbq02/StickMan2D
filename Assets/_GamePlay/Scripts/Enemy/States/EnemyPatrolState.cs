using UnityEngine;

// Đi qua lại quanh HomeX ± patrolDistance, nghỉ ở mỗi đầu; thấy player => Chase
public class EnemyPatrolState : EnemyState
{
    private float pauseTimer;

    public EnemyPatrolState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        pauseTimer = 0f;
    }

    public override void Update()
    {
        if (!enemy.CanAct) return;

        if (enemy.CanSeeTarget)
        {
            ChangeState(enemy.Chase);
            return;
        }

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f) enemy.Flip();
            return;
        }

        // Tới đầu đường tuần tra (theo hướng đang đi) => nghỉ rồi quay đầu
        float offset = enemy.transform.position.x - enemy.HomeX;
        if (offset * enemy.FacingDirection >= Data.patrolDistance)
            pauseTimer = Data.patrolPause;
    }

    public override void FixedUpdate()
    {
        if (!enemy.CanAct || pauseTimer > 0f)
        {
            enemy.SetVelocityX(0f);
            return;
        }

        // Gặp vực / tường trước khi tới đầu đường => nghỉ và quay đầu sớm
        if (!enemy.MoveForward(Data.patrolSpeed))
            pauseTimer = Data.patrolPause;
    }
}
