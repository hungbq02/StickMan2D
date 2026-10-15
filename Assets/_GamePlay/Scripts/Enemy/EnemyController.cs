using UnityEngine;

// Enemy cơ bản: Patrol -> Chase -> Attack, Hurt khi bị đánh. Chỉ số lấy từ EnemyDataSO.
// Lưu ý thứ tự component: đặt EnemyController TRƯỚC các StatusEffect để khi trúng đòn,
// state Attack thoát (trả màu sprite) trước khi FreezeEffect chụp màu gốc.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Damageable))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyDataSO data;
    [SerializeField] private Hitbox attackHitbox;
    [Tooltip("Layer mặt đất + tường (dò mép vực / vật cản phía trước)")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Color windupTint = new Color(1f, 0.35f, 0.35f);

    private StateMachine<EnemyState> machine;
    private Collider2D bodyCollider;
    private SpriteRenderer[] renderers;
    private Color[] baseColors;
    private StunEffect stunEffect;
    private FreezeEffect freezeEffect;

    public EnemyDataSO Data => data;
    public Rigidbody2D Rigidbody { get; private set; }
    public Damageable Damageable { get; private set; }
    public Health Health { get; private set; }
    public Transform Target { get; private set; }
    public int FacingDirection { get; private set; } = 1;
    public float HomeX { get; private set; }
    public float LastAttackTime { get; set; } = float.NegativeInfinity;

    public EnemyPatrolState Patrol { get; private set; }
    public EnemyChaseState Chase { get; private set; }
    public EnemyAttackState Attack { get; private set; }
    public EnemyHurtState Hurt { get; private set; }

    public EnemyState CurrentState => machine.CurrentState;
    public bool CanAct => (stunEffect == null || !stunEffect.IsStunned) && (freezeEffect == null || !freezeEffect.IsFrozen);
    public bool AttackReady => Time.time - LastAttackTime >= data.attackCooldown;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Damageable = GetComponent<Damageable>();
        Health = GetComponent<Health>();
        bodyCollider = GetComponent<Collider2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
        baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            baseColors[i] = renderers[i].color;
        stunEffect = GetComponent<StunEffect>();
        freezeEffect = GetComponent<FreezeEffect>();

        machine = new StateMachine<EnemyState>(name);
        Patrol = new EnemyPatrolState(this);
        Chase = new EnemyChaseState(this);
        Attack = new EnemyAttackState(this);
        Hurt = new EnemyHurtState(this);

        Damageable.OnDamaged += HandleDamaged;
    }

    private void OnDestroy()
    {
        if (Damageable != null)
            Damageable.OnDamaged -= HandleDamaged;
    }

    private void OnEnable()
    {
        if (data == null)
        {
            Debug.LogError($"[EnemyController] {name} chưa gán EnemyDataSO", this);
            enabled = false;
            return;
        }

        Health.SetMaxHealth(data.maxHealth);
        HomeX = transform.position.x;
        LastAttackTime = float.NegativeInfinity;
        if (Target == null)
        {
            // TODO(GS-02): lấy player từ game flow / service khi có
            var player = FindObjectOfType<PlayerController>();
            if (player != null) Target = player.transform;
        }
        machine.Initialize(Patrol);
    }

    private void OnDisable()
    {
        machine.Stop();
    }

    private void Update()
    {
        machine.Update();
    }

    private void FixedUpdate()
    {
        machine.FixedUpdate();
    }

    private void HandleDamaged(int damage, AttackData attackData)
    {
        if (!isActiveAndEnabled || !Health.IsAlive) return;

        // Bị đánh từ sau lưng => quay lại
        FaceTowards(attackData.attackerPosition.x);
        machine.ChangeState(Hurt);
    }

    public void ChangeState(EnemyState state) => machine.ChangeState(state);

    // ---- Target ----

    public bool HasTarget => Target != null && Target.gameObject.activeInHierarchy;

    public float TargetDistanceX => HasTarget ? Mathf.Abs(Target.position.x - transform.position.x) : float.PositiveInfinity;

    public bool TargetSameLevel => HasTarget && Mathf.Abs(Target.position.y - transform.position.y) <= data.maxHeightDifference;

    public bool CanSeeTarget => TargetSameLevel && TargetDistanceX <= data.detectRange;

    public bool TargetInAttackRange => TargetSameLevel && TargetDistanceX <= data.attackRange;

    // ---- Movement ----

    public void FaceTowards(float x)
    {
        float dx = x - transform.position.x;
        if (Mathf.Abs(dx) > 0.01f && Mathf.Sign(dx) != FacingDirection)
            Flip();
    }

    public void Flip()
    {
        FacingDirection *= -1;
        transform.Rotate(0f, 180f, 0f);
    }

    public void SetVelocityX(float x)
    {
        Rigidbody.velocity = new Vector2(x, Rigidbody.velocity.y);
    }

    // Đi theo hướng mặt; trả về false (và đứng lại) nếu phía trước là vực hoặc vật cản
    public bool MoveForward(float speed)
    {
        if (IsBlockedAhead())
        {
            SetVelocityX(0f);
            return false;
        }
        SetVelocityX(FacingDirection * speed);
        return true;
    }

    public bool IsBlockedAhead()
    {
        Bounds b = bodyCollider.bounds;
        float frontX = b.center.x + FacingDirection * (b.extents.x + 0.05f);

        // Vật cản ngang thân
        Vector2 wallOrigin = new Vector2(b.center.x, b.center.y);
        if (Physics2D.Raycast(wallOrigin, Vector2.right * FacingDirection, b.extents.x + 0.15f, obstacleLayer))
            return true;

        // Không còn đất phía trước chân => mép vực
        Vector2 groundOrigin = new Vector2(frontX, b.min.y + 0.1f);
        return !Physics2D.Raycast(groundOrigin, Vector2.down, 0.6f, obstacleLayer);
    }

    // ---- Attack ----

    public void PerformAttack()
    {
        LastAttackTime = Time.time;
        if (attackHitbox == null || data.attack == null) return;

        var attackData = new AttackData(data.attack, transform.position);
        attackData.sourceTeam = Damageable.Team;
        attackHitbox.CheckHit(attackData, FacingDirection);
    }

    public void SetWindupTint(bool on)
    {
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = on ? baseColors[i] * windupTint : baseColors[i];
    }

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Vector3 p = Application.isPlaying ? new Vector3(HomeX, transform.position.y) : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(p + Vector3.left * data.patrolDistance, p + Vector3.right * data.patrolDistance);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(transform.position, new Vector3(data.detectRange * 2f, data.maxHeightDifference * 2f));
    }
}
