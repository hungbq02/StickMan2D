// Choáng: chỉ bật cờ — AI / state machine của chủ đọc IsStunned để ngừng hành động
public class StunEffect : StatusEffect
{
    public override AttackEffect EffectType => AttackEffect.Stun;

    public bool IsStunned => IsActive;
}
