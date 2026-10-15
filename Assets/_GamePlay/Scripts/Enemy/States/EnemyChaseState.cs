// Đuổi theo player; trong tầm đánh + hết cooldown => Attack; player đi quá xa => Patrol
public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyController enemy) : base(enemy) { }

    public override void Update()
    {
        if (!enemy.CanAct) return;

        if (!enemy.HasTarget || enemy.TargetDistanceX > Data.loseRange || !enemy.TargetSameLevel)
        {
            ChangeState(enemy.Patrol);
            return;
        }

        enemy.FaceTowards(enemy.Target.position.x);

        if (enemy.TargetInAttackRange && enemy.AttackReady)
        {
            ChangeState(enemy.Attack);
        }
    }

    public override void FixedUpdate()
    {
        // Đã đứng trong tầm đánh (đang chờ cooldown) thì không ép sát thêm
        if (!enemy.CanAct || !enemy.HasTarget || enemy.TargetInAttackRange)
        {
            enemy.SetVelocityX(0f);
            return;
        }
        enemy.MoveForward(Data.chaseSpeed);
    }
}
