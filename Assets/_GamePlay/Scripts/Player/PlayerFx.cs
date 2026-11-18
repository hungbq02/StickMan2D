using Kore.Utils.Core;
using UnityEngine;

// Hiệu ứng chuyển động của player: bụi (nhảy / tiếp đất / chạy / dash / slide), tia trượt tường, vệt đòn, loé nòng.
// Bụi + tia: theo dõi state hiện tại (không sửa state). Vệt đòn / loé nòng: PlayerAnimationAttack gọi lúc ra đòn.
// Prefab dùng HitFx (pool, phát 1 lần rồi tự Recycle); sprite vẽ hướng +x, bụi có pivot ở chân.
[RequireComponent(typeof(PlayerController))]
public class PlayerFx : MonoBehaviour
{
    [Header("Bụi (pivot = chân)")]
    [SerializeField] private HitFx jumpDust;
    [SerializeField] private HitFx landDust;
    [SerializeField] private HitFx runDust;
    [SerializeField] private HitFx dashBurst;
    [SerializeField] private HitFx slideDust;
    [Tooltip("Pivot = điểm chạm tường")]
    [SerializeField] private HitFx wallSparks;

    [Header("Đòn (pivot giữa)")]
    [SerializeField] private HitFx fistSwoosh;
    [Tooltip("Theo bước combo 1..N")]
    [SerializeField] private HitFx[] swordSlashes;
    [SerializeField] private HitFx swordAir;
    [SerializeField] private HitFx muzzleFlash;
    [Tooltip("Tâm vệt đòn so với tâm collider (x theo hướng mặt)")]
    [SerializeField] private Vector2 attackOffset = new Vector2(0.55f, 0.1f);
    [Tooltip("Cỡ vệt ở bước combo cuối")]
    [SerializeField] private float finisherScale = 1.25f;

    [Header("Nhịp lặp")]
    [SerializeField] private float runDustInterval = 0.28f;
    [SerializeField] private float wallSparkInterval = 0.12f;

    private PlayerController player;
    private PlayerStateMachine stateMachine;
    private State lastState;
    private float repeatTimer;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        stateMachine = GetComponent<PlayerStateMachine>();
    }

    private void LateUpdate()
    {
        State current = stateMachine.CurrentState;
        if (current != lastState)
        {
            lastState = current;
            repeatTimer = 0f;
            if (current != null) OnEnter(current.StateType);
        }
        if (current == null) return;

        // lặp trong lúc chạy / trượt tường
        if (current.StateType == PlayerStateType.run && player.GroundCheck.IsGrounded)
            Repeat(runDustInterval, runDust, Feet());
        else if (current.StateType == PlayerStateType.wallSlide)
            Repeat(wallSparkInterval, wallSparks, WallPoint());
    }

    private void OnEnter(PlayerStateType type)
    {
        switch (type)
        {
            case PlayerStateType.jump:
                if (player.GroundCheck.IsGrounded) Spawn(jumpDust, Feet());
                else if (player.GroundCheck.IsTouchingWall) Spawn(wallSparks, WallPoint());
                break;
            case PlayerStateType.land: Spawn(landDust, Feet()); break;
            case PlayerStateType.dash: Spawn(dashBurst, Feet()); break;
            case PlayerStateType.slide: Spawn(slideDust, Feet()); break;
        }
    }

    /// <summary>Vệt đòn cận chiến lúc animation event ra đòn (step 0 = trên không).</summary>
    public void PlayAttack(WeaponType weapon, int step, int comboSteps)
    {
        HitFx prefab = null;
        if (weapon == WeaponType.Fighter) prefab = fistSwoosh;
        else if (weapon == WeaponType.Sword)
            prefab = step <= 0 ? swordAir
                : swordSlashes != null && swordSlashes.Length > 0 ? swordSlashes[Mathf.Min(step, swordSlashes.Length) - 1] : null;
        float scale = step > 0 && step >= comboSteps ? finisherScale : 1f;
        Body(out Vector2 center, out _);
        Spawn(prefab, center + new Vector2(attackOffset.x * player.FacingDirection, attackOffset.y), scale);
    }

    /// <summary>Loé nòng ở đầu nòng lúc bắn.</summary>
    public void PlayMuzzle(Vector2 muzzle) => Spawn(muzzleFlash, muzzle);

    private void Repeat(float interval, HitFx prefab, Vector2 point)
    {
        repeatTimer -= Time.deltaTime;
        if (repeatTimer > 0f) return;
        repeatTimer = interval;
        Spawn(prefab, point);
    }

    private Vector2 Feet()
    {
        Body(out Vector2 c, out Vector2 e);
        return new Vector2(c.x, c.y - e.y);
    }

    private Vector2 WallPoint()
    {
        Body(out Vector2 c, out Vector2 e);
        return new Vector2(c.x + e.x * player.FacingDirection, c.y - e.y * 0.2f);
    }

    // Tâm + nửa kích thước thân theo hình collider (không dùng bounds: dash tắt collider => bounds rỗng)
    private void Body(out Vector2 center, out Vector2 extents)
    {
        CapsuleCollider2D col = player.Collider;
        Vector3 scale = transform.lossyScale;
        center = transform.TransformPoint(col.offset); // player lật bằng xoay 180° -> TransformPoint
        extents = new Vector2(col.size.x * Mathf.Abs(scale.x), col.size.y * Mathf.Abs(scale.y)) * 0.5f;
    }

    private void Spawn(HitFx prefab, Vector2 point, float scale = 1f)
    {
        if (prefab == null) return;
        var fx = ObjectPool.Spawn(prefab, point, Quaternion.identity);
        fx.Init(player.FacingDirection, scale);
    }
}
