using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private WeaponManager weaponManager;

    // Cache hash theo key: chỉ ghép string ở lần đầu, các lần Play sau không cấp phát
    private static readonly Dictionary<(PlayerStateType, WeaponType), int> stateHashes = new Dictionary<(PlayerStateType, WeaponType), int>();
    private static readonly Dictionary<(int, WeaponType), int> comboHashes = new Dictionary<(int, WeaponType), int>();
    private static readonly Dictionary<WeaponType, int> attackHashes = new Dictionary<WeaponType, int>();
    private static readonly Dictionary<WeaponType, int> airAttackHashes = new Dictionary<WeaponType, int>();

    // State vừa Play và animator đã thực sự vào state đó chưa (dùng cho IsDone)
    private int lastPlayedHash;
    private bool lastPlayedStarted;
    private int lastPlayFrame = -1;

    void Awake()
    {
        animator = GetComponent<Animator>();
        weaponManager = GetComponentInParent<WeaponManager>();
    }

    // Phát animation theo loại vũ khí và combo (cho melee weapon): "attack{combo}_{weapon}"
    public void PlayAttack(WeaponType type, int combo)
    {
        var key = (combo, type);
        if (!comboHashes.TryGetValue(key, out int hash))
            comboHashes[key] = hash = CreateHash($"attack{combo}_{type}");
        PlayHash(hash, restart: true);
    }

    // Phát animation tấn công cho vũ khí (ranged weapon, vd súng): "attack_{weapon}"
    public void PlayAttack(WeaponType weaponType)
    {
        if (!attackHashes.TryGetValue(weaponType, out int hash))
            attackHashes[weaponType] = hash = CreateHash($"attack_{weaponType}");
        PlayHash(hash, restart: true);
    }

    // Phát animation tấn công trên không: "airAttack_{weapon}"
    public void PlayAirAttack(WeaponType weaponType)
    {
        if (!airAttackHashes.TryGetValue(weaponType, out int hash))
            airAttackHashes[weaponType] = hash = CreateHash($"airAttack_{weaponType}");
        PlayHash(hash, restart: true);
    }

    // Phát animation theo state và vũ khí hiện tại: "{state}_{weapon}"
    public void Play(PlayerStateType stateType)
    {
        var key = (stateType, weaponManager.CurrentType);
        if (!stateHashes.TryGetValue(key, out int hash))
            stateHashes[key] = hash = CreateHash($"{stateType}_{weaponManager.CurrentType}");
        PlayHash(hash);
    }

    public void SetSpeedAnimation(float speed)
    {
        animator.speed = speed;
    }

    // Animation vừa Play đã kết thúc chưa
    public bool IsDone()
    {
        // Animator.Play chỉ áp dụng ở lần animator update kế tiếp (sau Update): cùng frame với Play
        // thì state info vẫn là lần phát trước (kể cả khi phát lại đúng state đó) => chưa xong
        if (Time.frameCount == lastPlayFrame && !lastPlayedStarted)
            return false;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        // Animator.Play chỉ áp dụng ở lần update animator kế tiếp: trong lúc chờ,
        // state hiện tại vẫn là state cũ (có thể đã chạy hết) => chưa được coi là xong
        if (info.shortNameHash != lastPlayedHash)
            return lastPlayedStarted;

        lastPlayedStarted = true;
        return info.normalizedTime >= 1f && !animator.IsInTransition(0);
    }

    private int CreateHash(string stateName)
    {
        int hash = Animator.StringToHash(stateName);
        if (!animator.HasState(0, hash))
            Debug.LogError($"[AnimationController] Animator không có state '{stateName}'", this);
        return hash;
    }

    // restart = true: luôn phát lại từ đầu, kể cả khi đang ở đúng state đó (đòn đánh / phát bắn liên tiếp)
    // => animation event (Fire, FighterAttack...) chắc chắn bắn lại mỗi lần
    private void PlayHash(int hash, bool restart = false)
    {
        lastPlayedHash = hash;
        lastPlayFrame = Time.frameCount;
        // State không tồn tại thì coi như đã xong, tránh treo state đang chờ IsDone
        lastPlayedStarted = !animator.HasState(0, hash);
        if (restart)
            animator.Play(hash, 0, 0f);
        else
            animator.Play(hash);
    }
}
