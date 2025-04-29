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
}
