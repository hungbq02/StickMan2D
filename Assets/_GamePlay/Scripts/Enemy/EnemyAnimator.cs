using System.Collections.Generic;
using UnityEngine;

// Phát animation enemy theo tên ngắn: Play("walk") => state "walk_{animSet}" (clip do EnemyArtImporter sinh).
// Thiếu state => bỏ qua (enemy không có đủ bộ animation vẫn chạy được bằng timer của state).
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [Tooltip("Hậu tố tên state trong Animator: {anim}_{animSet}, vd Axeman")]
    [SerializeField] private string animSet;

    private Animator animator;
    private readonly Dictionary<string, int> hashes = new Dictionary<string, int>();
    private readonly Dictionary<string, float> lengths = new Dictionary<string, float>();

    private int lastPlayedHash;
    private bool lastPlayedStarted;
    private int lastPlayFrame = -1;

    public string AnimSet => animSet;
    public Animator Animator => animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                lengths[clip.name] = clip.length;
        }
    }

    // Bật lại (Spawn từ pool) => Animator về state mặc định, quên state đã Play lần trước
    private void OnEnable()
    {
        lastPlayedHash = 0;
        lastPlayedStarted = false;
    }

    public bool Has(string anim) => !string.IsNullOrEmpty(anim) && animator.HasState(0, Hash(anim));

    // Độ dài clip (giây, ở speed 1). Không có clip => 0
    public float GetLength(string anim)
    {
        if (string.IsNullOrEmpty(anim)) return 0f;
        return lengths.TryGetValue(StateName(anim), out float len) ? len : 0f;
    }

    // restart = true: phát lại từ đầu kể cả khi đang ở đúng state đó (đòn đánh liên tiếp)
    public bool Play(string anim, bool restart = false)
    {
        if (!Has(anim)) return false;
        int hash = Hash(anim);
        if (!restart && hash == lastPlayedHash) return true; // đang phát rồi (state gọi Play mỗi frame)
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

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.shortNameHash != lastPlayedHash)
            return lastPlayedStarted;

        lastPlayedStarted = true;
        return !info.loop && info.normalizedTime >= 1f && !animator.IsInTransition(0);
    }

    public void SetSpeed(float speed) => animator.speed = speed;

    private string StateName(string anim) => $"{anim}_{animSet}";

    private int Hash(string anim)
    {
        if (!hashes.TryGetValue(anim, out int hash))
            hashes[anim] = hash = Animator.StringToHash(StateName(anim));
        return hash;
    }
}
