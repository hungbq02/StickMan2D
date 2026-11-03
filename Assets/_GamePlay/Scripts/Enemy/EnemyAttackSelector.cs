using System;
using System.Collections.Generic;

// Chọn đòn: lọc theo tầm / phase / cooldown riêng từng đòn, rồi random theo weight.
// Tách khỏi MonoBehaviour để test được (EditMode).
public static class EnemyAttackSelector
{
    // isReady(attack): đòn đã hết cooldown chưa. random01: số ngẫu nhiên [0, 1).
    // ignoreRange = true: lên kế hoạch đòn tiếp theo (bỏ qua tầm, bỏ đòn lùi) — Chase sẽ tiến tới tầm của đòn đó.
    public static EnemyAttackSO Choose(IReadOnlyList<EnemyAttackSO> attacks, float distanceX, int phase,
                                       Func<EnemyAttackSO, bool> isReady, float random01, bool ignoreRange = false)
    {
        if (attacks == null) return null;
        float total = 0f;
        for (int i = 0; i < attacks.Count; i++)
        {
            EnemyAttackSO a = attacks[i];
            if (IsCandidate(a, distanceX, phase, isReady, ignoreRange)) total += a.weight;
        }
        if (total <= 0f) return null;

        float pick = random01 * total;
        EnemyAttackSO last = null;
        for (int i = 0; i < attacks.Count; i++)
        {
            EnemyAttackSO a = attacks[i];
            if (!IsCandidate(a, distanceX, phase, isReady, ignoreRange)) continue;
            last = a;
            pick -= a.weight;
            if (pick < 0f) return a;
        }
        return last; // random01 sát 1 + sai số float
    }

    // Tầm gần nhất trong các đòn tấn công của phase (bỏ đòn lùi) — lúc mọi đòn còn cooldown, enemy chờ ở tầm này
    public static float ShortestReach(IReadOnlyList<EnemyAttackSO> attacks, int phase)
    {
        float min = float.PositiveInfinity;
        if (attacks != null)
        {
            for (int i = 0; i < attacks.Count; i++)
            {
                EnemyAttackSO a = attacks[i];
                if (IsOffensive(a, phase)) min = Math.Min(min, a.maxRange);
            }
        }
        return float.IsPositiveInfinity(min) ? 0f : min;
    }

    private static bool IsOffensive(EnemyAttackSO a, int phase)
    {
        return a != null && a.weight > 0f && a.movement != EnemyAttackMovement.Retreat && phase >= a.minPhase && phase <= a.maxPhase;
    }

    private static bool IsCandidate(EnemyAttackSO a, float distanceX, int phase, Func<EnemyAttackSO, bool> isReady, bool ignoreRange)
    {
        if (a == null || a.weight <= 0f) return false;
        bool fits = ignoreRange ? IsOffensive(a, phase) : a.IsUsableAt(distanceX, phase);
        return fits && (isReady == null || isReady(a));
    }
}
