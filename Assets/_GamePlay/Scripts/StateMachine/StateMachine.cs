using UnityEngine;

// State machine dùng chung (không phải MonoBehaviour): owner tự gọi Update/FixedUpdate.
// - Chặn ChangeState lồng trong Enter/Exit của state khác
// - Editor: cảnh báo khi đổi state nhiều lần trong 1 frame (dấu hiệu transition vòng)
public class StateMachine<TState> where TState : class, IState
{
    private readonly string debugName;
    private bool isTransitioning;
#if UNITY_EDITOR
    private int lastChangeFrame = -1;
#endif

    public TState CurrentState { get; private set; }

    public StateMachine(string debugName = null)
    {
        this.debugName = debugName ?? GetType().Name;
    }

    public void Initialize(TState startState)
    {
        CurrentState = startState;
        CurrentState?.Enter();
    }

    public void ChangeState(TState newState)
    {
        if (newState == null) return;

        if (isTransitioning)
        {
            Debug.LogWarning($"[{debugName}] Bỏ qua ChangeState({newState.GetType().Name}) gọi trong lúc đang chuyển state");
            return;
        }

#if UNITY_EDITOR
        if (lastChangeFrame == Time.frameCount)
            Debug.LogWarning($"[{debugName}] Đổi state nhiều lần trong 1 frame: {CurrentState?.GetType().Name} -> {newState.GetType().Name}");
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

    public void Update() => CurrentState?.Update();
    public void FixedUpdate() => CurrentState?.FixedUpdate();
}
