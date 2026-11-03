using UnityEngine;

// Sinh GroundHazard ở mốc trúng của đòn có EnemyAttackSO.hazard:
// AtTarget = dưới chân mục tiêu (± hazardScatter); InFront = hàng nối tiếp phía trước, lan dần ra (hazardStagger);
// AroundSelf = 2 bên thân, lan dần ra (sóng chấn động).
public class EnemyHazardCaster : MonoBehaviour, IEnemyAttackHandler
{
    public void OnAttackHit(EnemyController enemy, EnemyAttackSO attack, int hitIndex)
    {
        if (attack.hazard == null || attack.hazardDamage == null) return;

        int count = Mathf.Max(1, attack.hazardCount);
        Vector3 self = enemy.transform.position;
        for (int i = 0; i < count; i++)
        {
            float delay = i * attack.hazardStagger;
            switch (attack.hazardPlacement)
            {
                case HazardPlacement.AtTarget:
                    float x = enemy.HasTarget ? enemy.Target.position.x : self.x + enemy.FacingDirection * attack.hazardStartDistance;
                    x += Random.Range(-attack.hazardScatter, attack.hazardScatter);
                    Spawn(enemy, attack, new Vector3(x, self.y), delay);
                    break;
                case HazardPlacement.InFront:
                    Spawn(enemy, attack, self + Vector3.right * (enemy.FacingDirection * (attack.hazardStartDistance + i * attack.hazardSpacing)), delay);
                    break;
                case HazardPlacement.AroundSelf:
                    float d = attack.hazardStartDistance + i * attack.hazardSpacing;
                    Spawn(enemy, attack, self + Vector3.right * d, delay);
                    Spawn(enemy, attack, self + Vector3.left * d, delay);
                    break;
            }
        }
    }

    private static void Spawn(EnemyController enemy, EnemyAttackSO attack, Vector3 position, float delay)
    {
        GroundHazard h = ObjectPool.Spawn(attack.hazard, position, Quaternion.identity);
        h.Arm(enemy.CreateAttackData(attack.hazardDamage), delay);
    }
}
