using System;
using System.Collections.Generic;

// Chọn đòn: lọc theo tầm / phase / cooldown riêng từng đòn, rồi random theo weight.
// Tách khỏi MonoBehaviour để test được (EditMode).
public static class EnemyAttackSelector
{
    // isReady(attack): đòn đã hết cooldown chưa. random01: số ngẫu nhiên [0, 1).
    public static EnemyAttackSO Choose(IReadOnlyList<EnemyAttackSO> attacks, float distanceX, int phase,
                                       Func<EnemyAttackSO, bool> isReady, float random01)
    {
        if (attacks == null) return null;
        float total = 0f;
        for (int i = 0; i < attacks.Count; i++)
        {
            EnemyAttackSO a = attacks[i];
            if (IsCandidate(a, distanceX, phase, isReady)) total += a.weight;
        }
        if (total <= 0f) return null;

        float pick = random01 * total;
        EnemyAttackSO last = null;
        for (int i = 0; i < attacks.Count; i++)
        {
            EnemyAttackSO a = attacks[i];
            if (!IsCandidate(a, distanceX, phase, isReady)) continue;
            last = a;
            pick -= a.weight;
            if (pick < 0f) return a;
        }
        return last; // random01 sát 1 + sai số float
    }

    // Tầm xa nhất của các đòn tấn công trong phase (kể cả đang cooldown; bỏ đòn lùi) — enemy tiến lại tới tầm này
    public static float MaxRange(IReadOnlyList<EnemyAttackSO> attacks, int phase)
    {
        float max = 0f;
        if (attacks == null) return max;
        for (int i = 0; i < attacks.Count; i++)
        {
            EnemyAttackSO a = attacks[i];
            if (a == null || a.weight <= 0f || a.movement == EnemyAttackMovement.Retreat) continue;
            if (phase < a.minPhase || phase > a.maxPhase) continue;
            max = Math.Max(max, a.maxRange);
        }
        return max;
    }

    private static bool IsCandidate(EnemyAttackSO a, float distanceX, int phase, Func<EnemyAttackSO, bool> isReady)
    {
        return a != null && a.weight > 0f && a.IsUsableAt(distanceX, phase) && (isReady == null || isReady(a));
    }
}
