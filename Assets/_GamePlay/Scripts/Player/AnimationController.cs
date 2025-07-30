using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private WeaponManager weaponManager;

    void Awake()
    {
        animator = GetComponent<Animator>();
        weaponManager = GetComponentInParent<WeaponManager>();
    }
    private static Dictionary<string, int> hashCache = new Dictionary<string, int>();

    // Lấy hash animation từ tên
    private int GetHash(string animName)
    {
        if (!hashCache.TryGetValue(animName, out int hash))
        {
            hash = Animator.StringToHash(animName);
            hashCache[animName] = hash;
        }
        return hash;
    }
    // Phát animation theo loại vũ khí và combo (cho melee weapon)
    public void PlayAttack(WeaponType type, int combo)
    {
        string anim = $"attack{combo}_{type}";
        animator.Play(GetHash(anim));
    }

    // Phát animation tấn công cho vũ khí (dùng cho ranged weapon, chẳng hạn như súng)
    public void PlayAttack(WeaponType weaponType)
    {
        string anim = $"attack_{weaponType}";
        animator.Play(GetHash(anim));
    }
    public void PlayAirAttack(WeaponType weaponType)
    {
        string anim = $"airAttack_{weaponType}";
        animator.Play(GetHash(anim));
    }
    //Play animation 
    public void Play(PlayerStateType stateType)
    {
        string animKey = $"{stateType}_{weaponManager.CurrentType}";
        animator.Play(animKey);
    }
    public void SetSpeedAnimation(float speed)
    {
        animator.speed = speed;
    }

    // Kiểm tra xem animation hiện tại đã kết thúc chưa
    public bool IsDone()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && !animator.IsInTransition(0);
    }
}
