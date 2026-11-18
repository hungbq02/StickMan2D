using Kore.Utils.Core;
using UnityEngine;

// Hết máu: tắt va chạm, phát animation chết, rồi mờ dần trong corpseLinger.
// Xác có trọng lượng: bị hất lùi theo hướng đòn kết liễu (deathSlide), chết trên không thì rơi xuống đất
// (body đã Kinematic + tắt collider => tự dò đất / tường bằng raycast, không xuyên).
// Khi bắt đầu mờ: tan thành khói — enemy thường 1 cụm, boss (deathBursts > 1) chuỗi nổ có rung, cụm cuối to nhất.
// Death.DespawnDelay (EnemyController gán) trả object về pool / tắt khi hết thời gian này.
public class EnemyDeadState : EnemyState
{
    private const float SlideTime = 0.45f;
    private const float Gravity = 30f;
    private const float BurstInterval = 0.18f;

    private float elapsed;
    private float deathLength;
    private Bounds body;          // khung thân lúc chết (collider tắt sau đó)
    private Vector2 startPos;
    private int slideDir;         // 0 = chưa biết (OnDamaged của đòn kết liễu tới sau OnDeath)
    private float slid;
    private float fallSpeed;
    private bool grounded;
    private int burstsDone;

    public EnemyDeadState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        elapsed = 0f;
        slideDir = 0;
        slid = 0f;
        fallSpeed = 0f;
        burstsDone = 0;
        body = enemy.BodyBounds;
        startPos = enemy.Rigidbody.position;
        grounded = GroundDistance(0.1f) >= 0f;
        enemy.SetWindupTint(false);
        enemy.SetCorpse(true);
        enemy.PlayAnim(Data.animDeath, restart: true);
        deathLength = enemy.AnimLength(Data.animDeath);
    }

    public override void Update()
    {
        float dt = Time.deltaTime;
        elapsed += dt;
        MoveCorpse(dt);

        if (elapsed > deathLength)
        {
            UpdateBursts(elapsed - deathLength);
            if (Data.corpseLinger > 0f)
                enemy.SetFade(1f - Mathf.Clamp01((elapsed - deathLength) / Data.corpseLinger));
        }
    }

    // ---- xác có trọng lượng ----

    private void MoveCorpse(float dt)
    {
        // body Kinematic vẫn trôi theo velocity: knockback của đòn kết liễu gán sau SetCorpse => xoá mỗi frame
        enemy.Rigidbody.velocity = Vector2.zero;
        Vector2 pos = enemy.Rigidbody.position;

        if (slideDir == 0)
            slideDir = enemy.LastHitFromX <= body.center.x ? 1 : -1; // bị đánh từ trái => văng sang phải
        if (Data.deathSlide > 0f && elapsed < SlideTime)
        {
            float target = Data.deathSlide * (1f - Mathf.Pow(1f - elapsed / SlideTime, 3f));
            float step = target - slid;
            if (step > 0f && !Blocked(pos, step))
            {
                pos.x += step * slideDir;
                slid = target;
            }
        }

        if (!grounded)
        {
            fallSpeed += Gravity * dt;
            float move = fallSpeed * dt;
            float dist = GroundDistance(move + 0.05f, pos);
            if (dist >= 0f && dist <= move)
            {
                pos.y -= dist;
                grounded = true;
                fallSpeed = 0f;
            }
            else pos.y -= move;
        }

        if (pos != enemy.Rigidbody.position)
        {
            enemy.Rigidbody.position = pos;
            enemy.transform.position = new Vector3(pos.x, pos.y, enemy.transform.position.z);
        }
    }

    // Khoảng cách từ chân xuống đất (−1 = không có đất trong tầm)
    private float GroundDistance(float range) => GroundDistance(range, enemy.Rigidbody.position);

    private float GroundDistance(float range, Vector2 pos)
    {
        Vector2 offset = pos - startPos;
        Vector2 feet = new Vector2(body.center.x + offset.x, body.min.y + offset.y + 0.05f);
        RaycastHit2D hit = Physics2D.Raycast(feet, Vector2.down, range + 0.05f, enemy.ObstacleLayer);
        return hit.collider != null ? Mathf.Max(0f, hit.distance - 0.05f) : -1f;
    }

    private bool Blocked(Vector2 pos, float step)
    {
        Vector2 offset = pos - startPos;
        Vector2 origin = new Vector2(body.center.x + offset.x, body.min.y + offset.y + Mathf.Min(0.3f, body.size.y * 0.25f));
        return Physics2D.Raycast(origin, Vector2.right * slideDir, body.extents.x + step, enemy.ObstacleLayer).collider != null;
    }

    // ---- tan thành khói ----

    private void UpdateBursts(float t)
    {
        int total = Mathf.Max(1, Data.deathBursts);
        while (burstsDone < total && t >= burstsDone * BurstInterval)
        {
            SpawnBurst(burstsDone, total);
            burstsDone++;
        }
    }

    private void SpawnBurst(int index, int total)
    {
        var reg = Service.Get<PrefabRegistrySO>();
        if (reg == null || reg.enemyDeathFx == null) return;
        Vector2 offset = enemy.Rigidbody.position - startPos;
        float h = body.size.y;
        float size = Mathf.Max(0.6f, h / 2f); // art khói ~2u
        // pivot sprite khói ở đáy cụm khói => đặt ở chỗ xác nằm, khói bốc lên (không lấn xuống dưới đất)
        Vector2 center = new Vector2(body.center.x + offset.x, body.min.y + offset.y);

        if (total == 1)
        {
            Spawn(reg.enemyDeathFx, center, size);
            return;
        }

        bool last = index == total - 1;
        if (last)
        {
            // cụm cuối to nhất + loé trắng, rung mạnh
            Spawn(reg.enemyDeathFx, center, size * 1.3f);
            if (reg.critFx != null) Spawn(reg.critFx, center + Vector2.up * h * 0.25f, size);
            CameraShake.Add(0.6f);
        }
        else
        {
            // xác nằm ngang: rải theo bề ngang ~ chiều cao thân, nửa dưới
            Vector2 p = center + new Vector2(Random.Range(-0.5f, 0.5f) * h, Random.Range(0f, 0.15f) * h);
            Spawn(reg.enemyDeathFx, p, size * Random.Range(0.55f, 0.8f));
            CameraShake.Add(0.3f);
        }
    }

    private void Spawn(HitFx prefab, Vector2 point, float scale)
    {
        var fx = ObjectPool.Spawn(prefab, point, Quaternion.identity);
        fx.Init(enemy.FacingDirection, scale);
    }

    public override void Exit()
    {
        enemy.SetCorpse(false);
        enemy.SetFade(1f);
    }
}
