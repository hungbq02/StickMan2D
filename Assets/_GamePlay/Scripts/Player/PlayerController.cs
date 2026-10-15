using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(WeaponManager))]
[RequireComponent(typeof(WeaponAttackManager))]
public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public PlayerStats stats;

    public PlayerInputHandler InputHandler { get; private set; }
    public GroundCheck GroundCheck { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public CapsuleCollider2D Collider { get; private set; }
    // Animation player, Animation combat
    public AnimationController AnimationController { get; private set; }
    public GhostSpriteSpawner GhostSpawner { get; private set; }

    // Get/Set WeaponType hiện tại
    public WeaponManager WeaponManager { get; private set; }
    // Get/Set AttackData hiện tại
    public WeaponAttackManager WeaponAttackManager { get; private set; }

    public IAttackHandler MeleeHandler { get; private set; }
    public IAttackHandler GunHandler { get; private set; }

    public PlayerStates States { get; private set; }
    private PlayerStateMachine stateMachine;

    public int FacingDirection { get; private set; } = 1; // 1 = right, -1 = left
    public int LastWallJumpDirection { get; set; } = 0; // 0 = reset, 1 = right, -1 = left

    public bool CanDash { get; private set; } = true;

    public Damageable Damageable { get; private set; }
    private StunEffect stunEffect;
    public bool IsStunned => stunEffect != null && stunEffect.IsStunned;

    // Coyote time: cho phép nhảy trong thời gian ngắn sau khi rời mặt đất mà chưa nhảy
    private bool coyoteAvailable;
    public bool CanGroundJump =>
        GroundCheck.IsGrounded ||
        (coyoteAvailable && Time.time - GroundCheck.LastGroundedTime <= stats.coyoteTime);

    // Kích thước collider ban đầu
    private Vector2 originalSizeCollider;
    private Vector2 originalOffsetCollider;

    void Awake()
    {
        GroundCheck = GetComponentInChildren<GroundCheck>();
        Rigidbody = GetComponent<Rigidbody2D>();
        Collider = GetComponent<CapsuleCollider2D>();
        InputHandler = GetComponent<PlayerInputHandler>();
        stateMachine = GetComponent<PlayerStateMachine>();
        WeaponManager = GetComponent<WeaponManager>();
        WeaponAttackManager = GetComponent<WeaponAttackManager>();
        AnimationController = GetComponentInChildren<AnimationController>();
        GhostSpawner = GetComponentInChildren<GhostSpriteSpawner>();
        Damageable = GetComponent<Damageable>();
        stunEffect = GetComponent<StunEffect>();

        originalSizeCollider = Collider.size;
        originalOffsetCollider = Collider.offset;

        WeaponManager.OnWeaponChanged += HandleWeaponChanged;
        if (Damageable != null)
            Damageable.OnDamaged += HandleDamaged;

        MeleeHandler = new MeleeAttackHandler(this, AnimationController);
        GunHandler = new GunAttackHandler(this, AnimationController);

        States = new PlayerStates(stateMachine, this);
    }

    private void OnDestroy()
    {
        if (WeaponManager != null)
            WeaponManager.OnWeaponChanged -= HandleWeaponChanged;
        if (Damageable != null)
            Damageable.OnDamaged -= HandleDamaged;
    }

    void Start()
    {
        stateMachine.Initialize(States.Idle);
    }

    void Update()
    {
        stateMachine.CurrentState?.Update();
    }

    void FixedUpdate()
    {
        stateMachine.CurrentState?.FixedUpdate();
    }

    public void CheckIfShouldFlip(float xInput)
    {
        if (xInput != 0 && Mathf.Sign(xInput) != FacingDirection)
        {
            Flip();
        }
    }

    public void Flip()
    {
        FacingDirection *= -1;
        transform.Rotate(0f, 180f, 0f);
    }

    public void SetVelocityY(float y)
    {
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, y);
    }

    public void SetVelocityX(float x)
    {
        Rigidbody.velocity = new Vector2(x, Rigidbody.velocity.y);
    }

    //Reset direction walljump
    public void ResetWallJumpLock()
    {
        LastWallJumpDirection = 0;
    }

    public void UseDash() => CanDash = false;
    public void ResetDash() => CanDash = true;

    public void RefreshCoyote() => coyoteAvailable = true;
    public void ConsumeCoyote() => coyoteAvailable = false;

    // Thu chiều cao collider (giữ chân ở vị trí cũ), dùng cho slide
    public void ShrinkColliderHeight(float heightMultiplier)
    {
        Collider.size = new Vector2(originalSizeCollider.x, originalSizeCollider.y * heightMultiplier);
        Collider.offset = new Vector2(originalOffsetCollider.x,
            originalOffsetCollider.y - originalSizeCollider.y * (1 - heightMultiplier) / 2f);
    }

    public void ResetCollider()
    {
        Collider.enabled = true;
        Collider.size = originalSizeCollider;
        Collider.offset = originalOffsetCollider;
    }

    // Trúng đòn => Hurt (rồi Stun nếu đòn có choáng). Đòn bị khiên chặn không tới đây.
    private void HandleDamaged(int damage, AttackData data)
    {
        if (!isActiveAndEnabled || stateMachine.CurrentState == null) return;
        Damageable.SetInvulnerable(stats.invincibilityDuration);
        stateMachine.ChangeState(States.Hurt);
    }

    private void HandleWeaponChanged(WeaponType weaponType)
    {
        if (stateMachine.CurrentState is AttackState attackState)
        {
            attackState.CancelAttack();
        }
        stateMachine.CurrentState?.OnWeaponChanged(AnimationController);
    }
}
