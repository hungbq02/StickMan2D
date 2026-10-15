// Chặn đòn trước khi Damageable trừ máu (vd: khiên). Đăng ký qua Damageable.AddBlocker.
public interface IDamageBlocker
{
    // true = đòn đã bị chặn hoàn toàn => chủ không mất máu, không nhận knockback/effect
    bool TryBlock(AttackData attackData);
}
