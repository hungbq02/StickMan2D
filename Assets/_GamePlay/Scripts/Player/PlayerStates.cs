// Mỗi state chỉ tạo 1 lần và được tái sử dụng => không cấp phát khi chuyển state
public class PlayerStates
{
    public readonly IdleState Idle;
    public readonly RunState Run;
    public readonly JumpState Jump;
    public readonly FallState Fall;
    public readonly LandState Land;
    public readonly AttackState Attack;
    public readonly AirAttackState AirAttack;
    public readonly DashState Dash;
    public readonly SlideState Slide;
    public readonly ClimbState Climb;
    public readonly WallSlideState WallSlide;
    public readonly WallJumpState WallJump;
    public readonly HurtState Hurt;
    public readonly StunState Stun;
    public readonly DeadState Dead;

    public PlayerStates(PlayerStateMachine stateMachine, PlayerController player)
    {
        Idle = new IdleState(stateMachine, player);
        Run = new RunState(stateMachine, player);
        Jump = new JumpState(stateMachine, player);
        Fall = new FallState(stateMachine, player);
        Land = new LandState(stateMachine, player);
        Attack = new AttackState(stateMachine, player);
        AirAttack = new AirAttackState(stateMachine, player);
        Dash = new DashState(stateMachine, player);
        Slide = new SlideState(stateMachine, player);
        Climb = new ClimbState(stateMachine, player);
        WallSlide = new WallSlideState(stateMachine, player);
        WallJump = new WallJumpState(stateMachine, player);
        Hurt = new HurtState(stateMachine, player);
        Stun = new StunState(stateMachine, player);
        Dead = new DeadState(stateMachine, player);
    }
}
