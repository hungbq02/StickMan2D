using System.Collections.Generic;
using UnityEngine;

// Component khác trên enemy (bắn đạn, sinh vùng nguy hiểm, triệu hồi...) nhận mốc trúng của đòn
public interface IEnemyAttackHandler
{
    void OnAttackHit(EnemyController enemy, EnemyAttackSO attack, int hitIndex);
}

// Enemy: Patrol -> Chase -> Attack (Telegraph -> Active -> Recovery) -> ..., Hurt khi bị đánh, PhaseChange theo % máu,
// Dead phát animation chết rồi Death mới trả về pool. Chỉ số + đòn lấy từ EnemyDataSO / EnemyAttackSO.
// Animation qua EnemyAnimator (child "Visual"); không có thì state vẫn chạy theo timer.
// Lưu ý thứ tự component: đặt EnemyController TRƯỚC các StatusEffect để khi trúng đòn,
// state Attack thoát (trả màu sprite) trước khi FreezeEffect chụp màu gốc.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Damageable))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyDataSO data;
    [Tooltip("Hitbox mặc định cho đòn có EnemyAttackSO.hitbox trống")]
    [SerializeField] private Hitbox attackHitbox;
    [Tooltip("Hitbox khác, gọi theo tên GameObject (EnemyAttackSO.hitbox)")]
    [SerializeField] private Hitbox[] namedHitboxes;
    [Tooltip("Layer mặt đất + tường (dò mép vực / vật cản phía trước)")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Color windupTint = new Color(1f, 0.35f, 0.35f);

    private StateMachine<EnemyState> machine;
    private Collider2D bodyCollider;
    private SpriteRenderer[] renderers;
    private Color[] baseColors;
    private StunEffect stunEffect;
    private FreezeEffect freezeEffect;
    private Death death;
    private Health targetHealth;
    private IEnemyAttackHandler[] attackHandlers;
    private RigidbodyType2D baseBodyType;
    private readonly Dictionary<EnemyAttackSO, float> attackUsedAt = new Dictionary<EnemyAttackSO, float>();
    private bool windup;
    private float fade = 1f;
    private ContactFilter2D obstacleFilter;
    private readonly RaycastHit2D[] castHits = new RaycastHit2D[4];

    public EnemyDataSO Data => data;
    public EnemyAnimator Anim { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public Damageable Damageable { get; private set; }
    public Health Health { get; private set; }
    public Transform Target { get; private set; }
    public int FacingDirection { get; private set; } = 1;
    public float HomeX { get; private set; }
    public float LastAttackTime { get; private set; } = float.NegativeInfinity;
    public int Phase { get; private set; }
    public int PendingPhase { get; private set; }

    public EnemyPatrolState Patrol { get; private set; }
    public EnemyChaseState Chase { get; private set; }
    public EnemyAttackState Attack { get; private set; }
    public EnemyHurtState Hurt { get; private set; }
    public EnemyDeadState Dead { get; private set; }
    public EnemyPhaseChangeState PhaseChange { get; private set; }
    public EnemyTauntState Taunt { get; private set; }

    public EnemyState CurrentState => machine.CurrentState;
    public bool CanAct => (stunEffect == null || !stunEffect.IsStunned) && (freezeEffect == null || !freezeEffect.IsFrozen);

    // Hệ số theo phase hiện tại (enrage / boss phase)
    public float SpeedMultiplier => data.GetPhase(Phase)?.moveSpeedMultiplier ?? 1f;
    public float TempoMultiplier => data.GetPhase(Phase)?.tempoMultiplier ?? 1f;
    public string IdleAnim
    {
        get
        {
            string over = data.GetPhase(Phase)?.idleAnim;
            return !string.IsNullOrEmpty(over) && Anim != null && Anim.Has(over) ? over : data.animIdle;
        }
    }
    public string RunAnim => Anim != null && Anim.Has(data.animRun) ? data.animRun : data.animWalk;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Damageable = GetComponent<Damageable>();
        Health = GetComponent<Health>();
        Anim = GetComponentInChildren<EnemyAnimator>();
        bodyCollider = GetComponent<Collider2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
        baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            baseColors[i] = renderers[i].color;
        stunEffect = GetComponent<StunEffect>();
        freezeEffect = GetComponent<FreezeEffect>();
        death = GetComponent<Death>();
        attackHandlers = GetComponentsInChildren<IEnemyAttackHandler>(true);
        baseBodyType = Rigidbody.bodyType;

        machine = new StateMachine<EnemyState>(name);
        Patrol = new EnemyPatrolState(this);
        Chase = new EnemyChaseState(this);
        Attack = new EnemyAttackState(this);
        Hurt = new EnemyHurtState(this);
        Dead = new EnemyDeadState(this);
        PhaseChange = new EnemyPhaseChangeState(this);
        Taunt = new EnemyTauntState(this);

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
        Health.OnDeath += HandleDeath;
        HomeX = transform.position.x;
        LastAttackTime = float.NegativeInfinity;
        attackUsedAt.Clear();
        Phase = PendingPhase = 0;
        windup = false;
        fade = 1f;
        ApplyTint();
        if (bodyCollider != null) bodyCollider.enabled = true;
        Rigidbody.bodyType = baseBodyType;

        // Death chờ hết animation chết + thời gian nằm xác rồi mới trả về pool / tắt
        if (death != null && Anim != null && Anim.Has(data.animDeath))
            death.DespawnDelay = Anim.GetLength(data.animDeath) + data.corpseLinger;

        if (Target == null)
        {
            // TODO(GS-02): lấy player từ game flow / service khi có
            var player = FindObjectOfType<PlayerController>();
            if (player != null) Target = player.transform;
        }
        if (Target != null && targetHealth == null)
            targetHealth = Target.GetComponent<Health>();
        machine.Initialize(Patrol);
    }

    private void OnDisable()
    {
        if (Health != null) Health.OnDeath -= HandleDeath;
        machine?.Stop();
    }

    private void Update()
    {
        machine.Update();
    }

    private void FixedUpdate()
    {
        machine.FixedUpdate();
    }

    private void HandleDeath()
    {
        // Death không có delay có thể đã tắt object trước khi tới đây
        if (isActiveAndEnabled)
            machine.ChangeState(Dead);
    }

    private void HandleDamaged(int damage, AttackData attackData)
    {
        if (!isActiveAndEnabled || !Health.IsAlive) return;
        EnemyState current = machine.CurrentState;
        if (current == Dead || current == PhaseChange) return;

        // Qua ngưỡng máu của phase mới => chuyển phase (ngắt cả đòn đang ra)
        int phase = data.PhaseForHealth(Health.Max > 0 ? (float)Health.Current / Health.Max : 1f);
        if (phase > Phase)
        {
            PendingPhase = phase;
            machine.ChangeState(PhaseChange);
            return;
        }

        bool flinch = data.flinch == EnemyFlinch.Always
                      || (data.flinch == EnemyFlinch.NotWhileAttacking && current != Attack);
        if (!flinch) return;

        // Bị đánh từ sau lưng => quay lại
        FaceTowards(attackData.attackerPosition.x);
        machine.ChangeState(Hurt);
    }

    public void ChangeState(EnemyState state) => machine.ChangeState(state);

    public void EnterPhase(int phase)
    {
        Phase = phase;
        ApplyTint();
    }

    // ---- Target ----

    // Player đã chết (đang chờ hồi sinh) thì không còn là mục tiêu => enemy quay về tuần tra
    public bool HasTarget => Target != null && Target.gameObject.activeInHierarchy && (targetHealth == null || targetHealth.IsAlive);

    // Mục tiêu vừa bị hạ (để taunt)
    public bool TargetDefeated => Target != null && targetHealth != null && !targetHealth.IsAlive;

    public float TargetDistanceX => HasTarget ? Mathf.Abs(Target.position.x - transform.position.x) : float.PositiveInfinity;

    public bool TargetSameLevel => HasTarget && Mathf.Abs(Target.position.y - transform.position.y) <= data.maxHeightDifference;

    public bool CanSeeTarget => TargetSameLevel && TargetDistanceX <= data.detectRange;

    // ---- Attack selection ----

    // Lúc mọi đòn còn cooldown, Chase chờ ở tầm gần nhất của các đòn trong phase
    public float ShortestReach => EnemyAttackSelector.ShortestReach(data.attacks, Phase);

    // Dùng được ngay: hết khoảng nghỉ chung giữa 2 đòn + hết cooldown riêng của đòn
    public bool IsAttackReady(EnemyAttackSO attack)
    {
        return Time.time - LastAttackTime >= data.attackInterval * TempoMultiplier && IsOffCooldown(attack);
    }

    public bool IsOffCooldown(EnemyAttackSO attack)
    {
        return !attackUsedAt.TryGetValue(attack, out float t) || Time.time - t >= attack.cooldown * TempoMultiplier;
    }

    // Đòn dùng được ngay (tầm, phase, cooldown) hoặc null
    public EnemyAttackSO ChooseAttack()
    {
        if (!TargetSameLevel) return null;
        return EnemyAttackSelector.Choose(data.attacks, TargetDistanceX, Phase, IsAttackReady, Random.value);
    }

    // Đòn tiếp theo định dùng (hết cooldown riêng, bỏ qua tầm) — Chase tiến tới tầm của nó
    public EnemyAttackSO PlanAttack()
    {
        return EnemyAttackSelector.Choose(data.attacks, TargetDistanceX, Phase, IsOffCooldown, Random.value, ignoreRange: true);
    }

    public void StartAttack(EnemyAttackSO attack)
    {
        Attack.Setup(attack);
        machine.ChangeState(Attack);
    }

    public void MarkAttackUsed(EnemyAttackSO attack)
    {
        LastAttackTime = Time.time;
        if (attack != null) attackUsedAt[attack] = Time.time;
    }

    // Mốc trúng thứ hitIndex của đòn: chém bằng hitbox (nếu đòn có damage) + báo cho các handler (đạn, vùng nguy hiểm...)
    public void ExecuteHit(EnemyAttackSO attack, int hitIndex)
    {
        if (attack.damage != null)
        {
            Hitbox hitbox = GetHitbox(attack.hitbox);
            if (hitbox != null)
                hitbox.CheckHit(CreateAttackData(attack.damage), FacingDirection);
        }
        for (int i = 0; i < attackHandlers.Length; i++)
            attackHandlers[i].OnAttackHit(this, attack, hitIndex);
    }

    public AttackData CreateAttackData(AttackDataSO so)
    {
        var attackData = new AttackData(so, transform.position);
        attackData.sourceTeam = Damageable.Team;
        return attackData;
    }

    public Hitbox GetHitbox(string hitboxName)
    {
        if (string.IsNullOrEmpty(hitboxName)) return attackHitbox;
        if (namedHitboxes != null)
            foreach (Hitbox h in namedHitboxes)
                if (h != null && h.name == hitboxName) return h;
        Debug.LogWarning($"[EnemyController] {name} không có hitbox '{hitboxName}'", this);
        return attackHitbox;
    }

    // ---- Animation ----

    public void PlayAnim(string anim, bool restart = false)
    {
        if (Anim != null) Anim.Play(anim, restart);
    }

    public float AnimLength(string anim) => Anim != null ? Anim.GetLength(anim) : 0f;

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
    public bool MoveForward(float speed) => MoveTowardsDirection(FacingDirection, speed);

    // Đi theo hướng bất kỳ (lùi không quay mặt); trả về false nếu hướng đó bị chặn
    public bool MoveTowardsDirection(int direction, float speed)
    {
        if (IsBlocked(direction))
        {
            SetVelocityX(0f);
            return false;
        }
        SetVelocityX(direction * speed);
        return true;
    }

    public bool IsBlockedAhead() => IsBlocked(FacingDirection);

    public bool IsBlocked(int direction)
    {
        Bounds b = bodyCollider.bounds;
        float frontX = b.center.x + direction * (b.extents.x + 0.05f);

        // Vật cản trên cả chiều cao thân (quét hình collider; bỏ mặt đất / trần: pháp tuyến gần thẳng đứng)
        obstacleFilter.SetLayerMask(obstacleLayer);
        obstacleFilter.useTriggers = false;
        int n = bodyCollider.Cast(Vector2.right * direction, obstacleFilter, castHits, 0.15f);
        for (int i = 0; i < n; i++)
            if (Mathf.Abs(castHits[i].normal.x) > 0.5f) return true;

        // Không còn đất phía trước chân => mép vực
        Vector2 groundOrigin = new Vector2(frontX, b.min.y + 0.1f);
        return !Physics2D.Raycast(groundOrigin, Vector2.down, 0.6f, obstacleLayer);
    }

    // Dịch chuyển tức thời (đòn Teleport); giữ nguyên độ cao
    public void TeleportToX(float x)
    {
        Rigidbody.position = new Vector2(x, Rigidbody.position.y);
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    // Chết: không va chạm / không rơi nữa, nằm yên tại chỗ
    public void SetCorpse(bool corpse)
    {
        Rigidbody.velocity = Vector2.zero;
        Rigidbody.bodyType = corpse ? RigidbodyType2D.Kinematic : baseBodyType;
        if (bodyCollider != null) bodyCollider.enabled = !corpse;
    }

    // ---- Tint: màu gốc × màu phase × (tô báo hiệu) × độ mờ xác ----

    public void SetWindupTint(bool on)
    {
        windup = on;
        ApplyTint();
    }

    public void SetFade(float alpha)
    {
        fade = alpha;
        ApplyTint();
    }

    private void ApplyTint()
    {
        Color phaseTint = data != null ? data.GetPhase(Phase)?.tint ?? Color.white : Color.white;
        for (int i = 0; i < renderers.Length; i++)
        {
            Color c = baseColors[i] * phaseTint;
            if (windup) c *= windupTint;
            c.a = baseColors[i].a * fade;
            renderers[i].color = c;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Vector3 p = Application.isPlaying ? new Vector3(HomeX, transform.position.y) : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(p + Vector3.left * data.patrolDistance, p + Vector3.right * data.patrolDistance);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(transform.position, new Vector3(data.detectRange * 2f, data.maxHeightDifference * 2f));
        if (data.keepAwayDistance > 0f)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(data.keepAwayDistance * 2f, 0.2f));
        }
    }
}
