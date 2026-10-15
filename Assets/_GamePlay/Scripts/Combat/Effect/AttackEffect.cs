// Hiệu ứng trạng thái đi kèm đòn đánh (xử lý bởi các StatusEffect).
// Knockback không nằm ở đây: mọi đòn đều đẩy theo AttackDataSO.knockbackForce (KnockbackEffect).
// Giá trị số được giữ cố định vì asset serialize theo số (2 từng là Knockback — không dùng lại).
public enum AttackEffect
{
    None = 0,
    Stun = 1,
    Burn = 3,
    Freeze = 4,
}
