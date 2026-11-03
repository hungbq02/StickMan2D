using System.Collections.Generic;
using UnityEngine;

// Phát animation enemy theo tên ngắn: Play("walk") => state "walk_{animSet}" (clip do EnemyArtImporter sinh).
// Thiếu state => bỏ qua (enemy không có đủ bộ animation vẫn chạy được bằng timer của state).
// Root shift: clip kết thúc với thân lệch khỏi tư thế đứng (đòn bước tới / bị giật lùi) => khi chuyển sang clip khác
// sau lúc clip đó đã chạy hết, bắn RootShift(khoảng lệch theo hướng mặt) để chủ dời nhân vật — thân không giật ngược.
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [System.Serializable]
    public struct ClipShift
    {
        public string anim;
        [Tooltip("Đơn vị thế giới, + = phía trước mặt")]
        public float shift;
    }

    [Tooltip("Hậu tố tên state trong Animator: {anim}_{animSet}, vd Axeman")]
    [SerializeField] private string animSet;
    [Tooltip("Sinh bởi Tools/Enemies/Import Enemy Art (endShift trong manifest) — không sửa tay")]
    [SerializeField] private ClipShift[] endShifts;
    [Tooltip("Clip đã chạy tới tỉ lệ này mới coi là 'xong' để áp root shift (bị ngắt giữa chừng thì không dời)")]
    [SerializeField, Range(0.5f, 1f)] private float shiftAtProgress = 0.85f;

    // Khoảng dời theo hướng mặt (đơn vị thế giới)
    public event System.Action<float> RootShift;

    private Animator animator;
    private readonly Dictionary<string, int> hashes = new Dictionary<string, int>();
    private readonly Dictionary<string, float> lengths = new Dictionary<string, float>();
    private Dictionary<int, float> shiftByHash;

    private int lastPlayedHash;
    private bool lastPlayedStarted;
    private int lastPlayFrame = -1;

    public string AnimSet => animSet;
    public Animator Animator => Init();

    private void Awake()
    {
        Init();
    }

    // Khởi tạo lười: component khác (EnemyController.OnEnable) có thể gọi trước khi Awake của component này chạy
    private Animator Init()
    {
        if (animator != null) return animator;
        animator = GetComponent<Animator>();
        if (animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                lengths[clip.name] = clip.length;
        }
        return animator;
    }

    // Bật lại (Spawn từ pool) => Animator về state mặc định, quên state đã Play lần trước
    private void OnEnable()
    {
        lastPlayedHash = 0;
        lastPlayedStarted = false;
    }

    public bool Has(string anim) => !string.IsNullOrEmpty(anim) && Init().HasState(0, Hash(anim));

    // Độ dài clip (giây, ở speed 1). Không có clip => 0
    public float GetLength(string anim)
    {
        if (string.IsNullOrEmpty(anim)) return 0f;
        Init();
        return lengths.TryGetValue(StateName(anim), out float len) ? len : 0f;
    }

    // restart = true: phát lại từ đầu kể cả khi đang ở đúng state đó (đòn đánh liên tiếp)
    public bool Play(string anim, bool restart = false)
    {
        if (!Has(anim)) return false;
        int hash = Hash(anim);
        if (!restart && hash == lastPlayedHash) return true; // đang phát rồi (state gọi Play mỗi frame)
        ApplyEndShift();
        lastPlayedHash = hash;
        lastPlayFrame = Time.frameCount;
        lastPlayedStarted = false;
        if (restart)
            animator.Play(hash, 0, 0f);
        else
            animator.Play(hash);
        return true;
    }

    // Animation vừa Play đã chạy hết chưa (clip loop thì không bao giờ xong)
    public bool IsDone()
    {
        // Animator.Play áp dụng ở lần animator update kế tiếp => cùng frame với Play thì chưa xong
        if (Time.frameCount == lastPlayFrame && !lastPlayedStarted)
            return false;

        AnimatorStateInfo info = Init().GetCurrentAnimatorStateInfo(0);
        if (info.shortNameHash != lastPlayedHash)
            return lastPlayedStarted;

        lastPlayedStarted = true;
        return !info.loop && info.normalizedTime >= 1f && !animator.IsInTransition(0);
    }

    public void SetSpeed(float speed) => Init().speed = speed;

    public void SetEndShifts(ClipShift[] shifts)
    {
        endShifts = shifts;
        shiftByHash = null;
    }

    // Clip đang phát (đúng clip đã Play gần nhất) đã chạy gần hết và có root shift => báo chủ dời nhân vật
    private void ApplyEndShift()
    {
        if (lastPlayedHash == 0 || RootShift == null || endShifts == null || endShifts.Length == 0) return;
        if (shiftByHash == null)
        {
            shiftByHash = new Dictionary<int, float>();
            foreach (ClipShift c in endShifts)
                if (!string.IsNullOrEmpty(c.anim) && Mathf.Abs(c.shift) > 0.001f)
                    shiftByHash[Hash(c.anim)] = c.shift;
        }
        if (!shiftByHash.TryGetValue(lastPlayedHash, out float shift)) return;
        AnimatorStateInfo info = Init().GetCurrentAnimatorStateInfo(0);
        if (info.shortNameHash != lastPlayedHash || info.loop || info.normalizedTime < shiftAtProgress) return;
        RootShift(shift);
    }

    private string StateName(string anim) => $"{anim}_{animSet}";

    private int Hash(string anim)
    {
        if (!hashes.TryGetValue(anim, out int hash))
            hashes[anim] = hash = Animator.StringToHash(StateName(anim));
        return hash;
    }
}
