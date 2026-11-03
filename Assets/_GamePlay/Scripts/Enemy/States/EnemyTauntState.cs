// Vừa hạ được player: đứng phát animation khiêu khích 1 lần rồi về tuần tra (player hồi sinh trong tầm => đuổi luôn)
public class EnemyTauntState : EnemyState
{
    public EnemyTauntState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.SetVelocityX(0f);
        enemy.PlayAnim(Data.animTaunt, restart: true);
    }

    public override void Update()
    {
        if (enemy.CanSeeTarget)
        {
            ChangeState(enemy.Chase);
            return;
        }
        if (enemy.Anim == null || enemy.Anim.IsDone())
            ChangeState(enemy.Patrol);
    }

    public override void FixedUpdate()
    {
        enemy.SetVelocityX(0f);
    }
}
