public interface IAttackHandler
{
    void Attack();
    void Update();
    void Cancel(); //Nếu player bị trúng đòn => bạn chỉ cần gọi attackHandler.Cancel()
    /// <summary>
    /// example:
    /// attackHandler?.Cancel();
    ///stateMachine.ChangeState(new StunState(...));
    /// </summary>
    bool IsDone { get; }

    // Bước đòn đang phát (1..N) và số lần đã bắt đầu một bước (tăng mỗi lần phát đòn mới => AttackState biết để lao tới)
    int CurrentStep { get; }
    int StepSerial { get; }
    // Được ngắt phần thu đòn bằng Jump / Dash / Slide
    bool CanActionCancel { get; }
    // Được ngắt phần thu đòn bằng giữ hướng di chuyển
    bool CanMoveCancel { get; }
    // Animation event trúng đòn của bước hiện tại (melee)
    void OnHitFrame();
    // Rời đòn chủ động (ngắt / hết clip) — melee giữ lại bước combo để bấm tiếp trong cửa sổ combo
    void Release();
}
