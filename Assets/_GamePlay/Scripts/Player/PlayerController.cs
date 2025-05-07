using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public PlayerStats stats;
    public PlayerInputHandler InputHandler { get; private set; }
    private PlayerStateMachine stateMachine;
    public GroundCheck GroundCheck { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public CapsuleCollider2D Collider { get; private set; }

    [Tooltip("Kích thước collider ban đầu")]
    [HideInInspector] public Vector2 originalSizeCollider;
    [HideInInspector] public Vector2 originalOffsetCollider;

    [Tooltip("Animation player, Animation combat")]
    public AnimationController AnimationController { get; private set; }

    [Tooltip("Get/Set WeaponType hiện tại ")]
    public WeaponManager weaponManager { get; private set; }

    public int FacingDirection { get; private set; } = 1; // 1 = right, -1 = left
    public int LastWallJumpDirection { get; set; } = 0; // 0 = reset, 1 = right, -1 = left

    public IAttackHandler meleeHandler;
    public IAttackHandler gunHandler;

    public bool canDash { get; set; } = true;

    [Tooltip("Chỉ mình các vũ khí cận chiến (fighter, sword), riêng Gun Data được gắn ở Bullet")]
    public List<WeaponAttackDataSO> weaponAttackDataList;

    //Get current WeaponAttackDataSO 
    private WeaponAttackDataSO currentWeaponAttackProfile;


    private AttackDataSO currentAttackData;
    public AttackDataSO CurrentAttackData => currentAttackData;

    public AttackDataSO GetCurrentAttackData(int comboIndex = 0)
    {
        if (currentWeaponAttackProfile != null && comboIndex >= 0 && comboIndex < currentWeaponAttackProfile.attackDataList.Count)
        {
            Debug.Log("Current Attack Data: " + currentWeaponAttackProfile.attackDataList[comboIndex]);
            return currentWeaponAttackProfile.attackDataList[comboIndex];
        }
        return null;
    }
    public WeaponAttackDataSO GetCurrentWeaponAttackProfile()
    {
        Debug.Log("Current Weapon Type: " + weaponManager.CurrentType);
        return weaponAttackDataList.Find(p => p.weaponType == weaponManager.CurrentType);
    }

    public void SetCurrentAttackData(AttackDataSO data)
    {
        currentAttackData = data;
        Debug.Log("Current Attack Data: " + currentAttackData);
    }



    void Awake()
    {
        GroundCheck = GetComponentInChildren<GroundCheck>();
        Rigidbody = GetComponent<Rigidbody2D>();
        Collider = GetComponent<CapsuleCollider2D>();
        InputHandler = GetComponent<PlayerInputHandler>();
        stateMachine = GetComponent<PlayerStateMachine>();
        weaponManager = GetComponent<WeaponManager>();
        AnimationController = GetComponentInChildren<AnimationController>();

        weaponManager.OnWeaponChanged += HandleWeaponChanged;


        meleeHandler = new MeleeAttackHandler(this, AnimationController);
        gunHandler = new GunAttackHandler(this, AnimationController);
    }
    private void OnDestroy()
    {
        weaponManager.OnWeaponChanged -= HandleWeaponChanged;
    }
    void Start()
    {
        stateMachine.Initialize(new IdleState(stateMachine, this, PlayerStateType.idle));
        currentWeaponAttackProfile = GetCurrentWeaponAttackProfile();
        currentAttackData = GetCurrentAttackData();

        originalSizeCollider = Collider.size;
        originalOffsetCollider = Collider.offset;
    }

    void Update()
    {
        stateMachine.CurrentState?.Update();
        InputHandler.ResetInputs();


        if (Input.GetKeyDown(KeyCode.F1))
            weaponManager.SetWeaponType(WeaponType.Fighter);
        else if (Input.GetKeyDown(KeyCode.F2))
            weaponManager.SetWeaponType(WeaponType.Sword);
        else if (Input.GetKeyDown(KeyCode.F3))
            weaponManager.SetWeaponType(WeaponType.Gun);

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
    public void ResetCollider()
    {
        Collider.enabled = true;
        Collider.size = originalSizeCollider;
        Collider.offset = originalOffsetCollider;
    }
    private void HandleWeaponChanged()
    {
        currentWeaponAttackProfile = GetCurrentWeaponAttackProfile();

        // Lấy state hiện tại
        var currentState = stateMachine.CurrentState;
        if (currentState != null)
        {
            // Gọi lại animation tương ứng với WeaponType mới
            AnimationController.Play(currentState.StateType);
        }
        // Lấy AttackData tương ứng với WeaponType mới
        currentAttackData = GetCurrentAttackData();
    }


}
