using UnityEngine;

// Đóng băng: giữ chặt vị trí (kể cả knockback), tạm dừng animator và tô màu băng cho sprite
[RequireComponent(typeof(Rigidbody2D))]
public class FreezeEffect : StatusEffect
{
    [SerializeField] private Color freezeTint = new Color(0.55f, 0.85f, 1f);

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer[] renderers;
    private Color[] originalColors;
    private RigidbodyConstraints2D originalConstraints;
    private float originalAnimatorSpeed;

    public override AttackEffect EffectType => AttackEffect.Freeze;

    public bool IsFrozen => IsActive;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[renderers.Length];
    }

    protected override void Begin()
    {
        originalConstraints = rb.constraints;
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        if (animator != null)
        {
            originalAnimatorSpeed = animator.speed;
            animator.speed = 0f;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
            renderers[i].color = originalColors[i] * freezeTint;
        }
    }

    protected override void End()
    {
        rb.constraints = originalConstraints;

        if (animator != null)
            animator.speed = originalAnimatorSpeed;

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = originalColors[i];
    }
}
