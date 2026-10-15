using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public State CurrentState { get; private set; }

    private bool isTransitioning;
#if UNITY_EDITOR
    private int lastChangeFrame = -1;
#endif

    public void Initialize(State startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public void ChangeState(State newState)
    {
        if (newState == null) return;

        // Không cho đổi state lồng trong Enter/Exit của state khác
        if (isTransitioning)
        {
            Debug.LogWarning($"[PlayerStateMachine] Bỏ qua ChangeState({newState.GetType().Name}) gọi trong lúc đang chuyển state");
            return;
        }

#if UNITY_EDITOR
        if (lastChangeFrame == Time.frameCount)
            Debug.LogWarning($"[PlayerStateMachine] Đổi state nhiều lần trong 1 frame: {CurrentState?.GetType().Name} -> {newState.GetType().Name}");
        lastChangeFrame = Time.frameCount;
#endif

        isTransitioning = true;
        try
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }
        finally
        {
            isTransitioning = false;
        }
    }
}
