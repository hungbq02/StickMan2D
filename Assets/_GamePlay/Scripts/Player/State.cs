using UnityEngine;

public abstract class State
{
    protected PlayerStateMachine stateMachine;
    protected PlayerController player;
    protected PlayerStateType stateType;
    public PlayerStateType StateType => stateType;


    public State(PlayerStateMachine stateMachine, PlayerController player, PlayerStateType stateType)
    {
        this.stateMachine = stateMachine;
        this.player = player;
        this.stateType = stateType;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
    public virtual void OnWeaponChanged(AnimationController animationController)
    {
        animationController.Play(StateType);
    }

}
