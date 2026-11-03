using UnityEngine;

// Chặn đòn đến từ phía trước trong lúc enemy đang ra đòn có EnemyAttackSO.guardFront (giơ khiên).
// Đăng ký làm IDamageBlocker của Damageable; đòn bị chặn => Damageable.OnBlocked, không mất máu / knockback.
[RequireComponent(typeof(EnemyController))]
public class EnemyGuard : MonoBehaviour, IDamageBlocker
{
    private EnemyController enemy;
    private Damageable damageable;

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
        damageable = GetComponent<Damageable>();
    }

    private void OnEnable()
    {
        damageable.AddBlocker(this);
    }

    private void OnDisable()
    {
        damageable.RemoveBlocker(this);
    }

    public bool TryBlock(AttackData attackData)
    {
        EnemyAttackSO current = enemy.Attack.Current;
        if (enemy.CurrentState != enemy.Attack || current == null || !current.guardFront || !enemy.Attack.IsActiveStage)
            return false;
        float side = attackData.attackerPosition.x - enemy.transform.position.x;
        return Mathf.Approximately(side, 0f) || Mathf.Sign(side) == enemy.FacingDirection;
    }
}
