using Kore.Utils.Core;
using UnityEngine;

// Hết máu: nằm yên (tắt va chạm), phát animation chết, rồi mờ dần trong corpseLinger.
// Death.DespawnDelay (EnemyController gán) trả object về pool / tắt khi hết thời gian này.
public class EnemyDeadState : EnemyState
{
    private float elapsed;
    private float deathLength;
    private Bounds body;
    private bool fxPlayed;

    public EnemyDeadState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        elapsed = 0f;
        fxPlayed = false;
        body = enemy.BodyBounds;
        enemy.SetWindupTint(false);
        enemy.SetCorpse(true);
        enemy.PlayAnim(Data.animDeath, restart: true);
        deathLength = enemy.AnimLength(Data.animDeath);
    }

    public override void Update()
    {
        elapsed += Time.deltaTime;
        if (!fxPlayed && elapsed > deathLength)
        {
            fxPlayed = true;
            PlayDeathFx();
        }
        if (Data.corpseLinger > 0f && elapsed > deathLength)
            enemy.SetFade(1f - Mathf.Clamp01((elapsed - deathLength) / Data.corpseLinger));
    }

    // Xác tan thành khói: hiệu ứng ở thân (xác nằm thấp -> lệch xuống), cỡ theo chiều cao thân (art ~2u)
    private void PlayDeathFx()
    {
        var reg = Service.Get<PrefabRegistrySO>();
        if (reg == null || reg.enemyDeathFx == null) return;
        Vector2 point = new Vector2(body.center.x, body.min.y + body.size.y * 0.35f);
        var fx = ObjectPool.Spawn(reg.enemyDeathFx, point, Quaternion.identity);
        fx.Init(enemy.FacingDirection, Mathf.Max(0.6f, body.size.y / 2f));
    }

    public override void Exit()
    {
        enemy.SetCorpse(false);
        enemy.SetFade(1f);
    }
}
