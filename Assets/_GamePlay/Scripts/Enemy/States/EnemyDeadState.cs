using UnityEngine;

// Hết máu: nằm yên (tắt va chạm), phát animation chết, rồi mờ dần trong corpseLinger.
// Death.DespawnDelay (EnemyController gán) trả object về pool / tắt khi hết thời gian này.
public class EnemyDeadState : EnemyState
{
    private float elapsed;
    private float deathLength;

    public EnemyDeadState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        elapsed = 0f;
        enemy.SetWindupTint(false);
        enemy.SetCorpse(true);
        enemy.PlayAnim(Data.animDeath, restart: true);
        deathLength = enemy.AnimLength(Data.animDeath);
    }

    public override void Update()
    {
        elapsed += Time.deltaTime;
        if (Data.corpseLinger > 0f && elapsed > deathLength)
            enemy.SetFade(1f - Mathf.Clamp01((elapsed - deathLength) / Data.corpseLinger));
    }

    public override void Exit()
    {
        enemy.SetCorpse(false);
        enemy.SetFade(1f);
    }
}
